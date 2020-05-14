using Dapper;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.BirthdayReward.Infrastructure.Models;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.BirthdayReward.Infrastructure
{
    public class BirthdayRepository : PostgresRepository, IBirthdayRepository
    {
        public BirthdayRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task<IEnumerable<RewardedUser>> RewardEligibleUsersAsync(long rewardAmount)
        {
            using (var connection = Connection)
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var eligibleUsers = await connection.QueryAsync<EligibleUserDto>(
                        @"UPDATE attributes.birthdays SET last_reward_at = CURRENT_TIMESTAMP
                        WHERE (last_reward_at IS NULL OR last_reward_at <= CURRENT_TIMESTAMP - INTERVAL '360 DAYS')
                        AND (
                            (birthday + (INTERVAL '1 YEAR' * (date_part('year', CURRENT_DATE) - date_part('year', birthday))))
                            BETWEEN CURRENT_DATE - 2 AND CURRENT_DATE
                        )
                        RETURNING user_id;"
                    );

                    var rewardedUsers = await connection.QueryAsync<RewardedUserDto>(
                        @"UPDATE users.users SET taypoint_count = taypoint_count + @PointsToAdd
                        WHERE user_id = ANY(@UserIds) RETURNING user_id, taypoint_count;",
                        new
                        {
                            PointsToAdd = rewardAmount,
                            UserIds = eligibleUsers.Select(u => u.user_id).ToList()
                        }
                    );

                    transaction.Commit();

                    return rewardedUsers.Select(u => new RewardedUser(new SnowflakeId(u.user_id), u.taypoint_count));
                }
            }
        }
    }
}
