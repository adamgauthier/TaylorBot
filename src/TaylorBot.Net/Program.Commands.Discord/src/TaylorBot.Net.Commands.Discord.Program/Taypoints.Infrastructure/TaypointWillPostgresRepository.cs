using Dapper;
using Discord;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Taypoints.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Taypoints.Infrastructure
{
    public class TaypointWillPostgresRepository : PostgresRepository, ITaypointWillRepository
    {
        public TaypointWillPostgresRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        private class WillDto
        {
            public string user_id { get; set; }
            public DateTimeOffset max_last_spoke_at { get; set; }
        }

        public async ValueTask<IReadOnlyCollection<Will>> GetWillsWithBeneficiaryAsync(IUser beneficiary)
        {
            using var connection = Connection;

            var willDtos = await connection.QueryAsync<WillDto>(
                @"SELECT DISTINCT ON (user_id) user_id, last_spoke_at AS max_last_spoke_at
                FROM guilds.guild_members
                WHERE last_spoke_at IS NOT NULL AND user_id IN (
                  SELECT owner_user_id FROM users.taypoint_wills WHERE beneficiary_user_id = @UserId
                )
                ORDER BY user_id, last_spoke_at DESC;",
                new
                {
                    UserId = beneficiary.Id.ToString()
                }
            );

            return willDtos.Select(w => new Will(
                ownerUserId: new SnowflakeId(w.user_id),
                ownerLatestSpokeAt: w.max_last_spoke_at
            )).ToList();
        }

        private class TransferDto
        {
            public string user_id { get; set; }
            public long taypoint_count { get; set; }
            public long original_taypoint_count { get; set; }
        }

        public async ValueTask<IReadOnlyCollection<Transfer>> TransferAllPointsAsync(IReadOnlyCollection<SnowflakeId> fromUserIds, IUser toUser)
        {
            using var connection = Connection;

            var transferDtos = await connection.QueryAsync<TransferDto>(
                @"WITH
                old_u AS (
                    SELECT user_id, taypoint_count FROM users.users
                    WHERE user_id = @ReceiverId OR user_id = ANY(@FromUserIds) FOR UPDATE
                ),
                sum_gifters AS (
                    SELECT SUM(taypoint_count) AS sum_taypoints FROM old_u
                    WHERE user_id <> @ReceiverId
                )
                UPDATE users.users AS u
                SET taypoint_count = (CASE
                    WHEN u.user_id = @ReceiverId
                    THEN u.taypoint_count + sum_gifters.sum_taypoints
                    ELSE 0
                END)
                FROM old_u, sum_gifters
                WHERE u.user_id = old_u.user_id
                RETURNING u.user_id, u.taypoint_count, old_u.taypoint_count AS original_taypoint_count;",
                new
                {
                    ReceiverId = toUser.Id.ToString(),
                    FromUserIds = fromUserIds.Select(u => u.Id.ToString()).ToList()
                }
            );

            return transferDtos.Select(t => new Transfer(
                userId: new SnowflakeId(t.user_id),
                taypointCount: t.taypoint_count,
                originalTaypointCount: t.original_taypoint_count
            )).ToList();
        }

        public async ValueTask RemoveWillsAsync(IReadOnlyCollection<SnowflakeId> ownerUserIds, IUser beneficiary)
        {
            using var connection = Connection;

            await connection.ExecuteAsync(
                @"DELETE FROM users.taypoint_wills
                WHERE beneficiary_user_id = @BeneficiaryUserId AND owner_user_id = ANY(@OwnerUserIds);",
                new
                {
                    BeneficiaryUserId = beneficiary.Id.ToString(),
                    OwnerUserIds = ownerUserIds.Select(u => u.Id.ToString()).ToList()
                }
            );
        }
    }
}
