using Dapper;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.MinutesTracker.Domain;
using TaylorBot.Net.Core.Infrastructure;
using System;

namespace TaylorBot.Net.MinutesTracker.Infrastructure
{
    public class MinutesRepository : PostgresRepository, IMinuteRepository
    {
        public MinutesRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task AddMinutesToActiveMembersAsync(long minutesToAdd, TimeSpan minimumTimeSpanSinceLastSpoke, long minutesRequiredForReward, long pointsReward)
        {
            using (var connection = Connection)
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    await connection.ExecuteAsync(
                        @"UPDATE guilds.guild_members
                        SET minute_count = minute_count + @MinutesToAdd
                        WHERE TO_TIMESTAMP(last_spoke_at::double precision / 1000::double precision) > CURRENT_TIMESTAMP - @MinimumTimeSpanSinceLastSpoke;",
                        new
                        {
                            MinutesToAdd = minutesToAdd,
                            MinimumTimeSpanSinceLastSpoke = minimumTimeSpanSinceLastSpoke
                        }
                    );

                    await connection.ExecuteAsync(
                        @"UPDATE users.users SET
                           taypoint_count = taypoint_count + @PointsReward
                        WHERE user_id IN (
                            SELECT user_id FROM guilds.guild_members
                            WHERE minute_count >= minutes_milestone + @MinutesRequiredForReward
                        );",
                        new
                        {
                            PointsReward = pointsReward,
                            MinutesRequiredForReward = minutesRequiredForReward
                        }
                    );

                    await connection.ExecuteAsync(
                        @"UPDATE guilds.guild_members SET
                           minutes_milestone = (minute_count - (minute_count % @MinutesRequiredForReward)),
                           experience = experience + @PointsReward
                        WHERE minute_count >= minutes_milestone + @MinutesRequiredForReward;",
                        new
                        {
                            PointsReward = pointsReward,
                            MinutesRequiredForReward = minutesRequiredForReward
                        }
                    );

                    transaction.Commit();
                }
            }
        }
    }
}
