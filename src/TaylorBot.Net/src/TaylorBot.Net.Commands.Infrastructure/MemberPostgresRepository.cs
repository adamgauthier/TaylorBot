using Dapper;
using Discord;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class MemberPostgresRepository : PostgresRepository, IMemberRepository
    {
        public MemberPostgresRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task<bool> AddOrUpdateMemberAsync(IGuildUser member)
        {
            using var connection = Connection;

            var experience = await connection.QuerySingleAsync<long>(
                @"INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at) VALUES (@GuildId, @UserId, @FirstJoinedAt)
                ON CONFLICT (guild_id, user_id) DO UPDATE SET
                    alive = TRUE,
                    first_joined_at = CASE
                        WHEN guild_members.first_joined_at IS NULL AND excluded.first_joined_at IS NOT NULL
                        THEN excluded.first_joined_at
                        ELSE guild_members.first_joined_at
                    END,
                    experience = guild_members.experience + 1
                RETURNING experience;",
                new
                {
                    GuildId = member.GuildId.ToString(),
                    UserId = member.Id.ToString(),
                    FirstJoinedAt = member.JoinedAt
                }
            );

            return experience == 0;
        }
    }
}
