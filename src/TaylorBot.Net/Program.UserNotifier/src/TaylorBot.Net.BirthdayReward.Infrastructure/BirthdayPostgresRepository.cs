using Dapper;
using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.BirthdayReward.Infrastructure;

public class BirthdayPostgresRepository(ILogger<BirthdayPostgresRepository> logger, PostgresConnectionFactory postgresConnectionFactory) : IBirthdayRepository
{
    private record EligibleUserDto(string user_id);

    private record RewardedUserDto(string user_id, long taypoint_count);

    public async ValueTask<List<RewardedUser>> RewardEligibleUsersAsync(long rewardAmount)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var eligibleUsers = await connection.QueryAsync<EligibleUserDto>(
            """
            UPDATE attributes.birthdays SET last_reward_at = CURRENT_TIMESTAMP
            WHERE (last_reward_at IS NULL OR last_reward_at <= CURRENT_TIMESTAMP - INTERVAL '360 DAYS')
            AND (
                (birthday + (INTERVAL '1 YEAR' * (date_part('year', CURRENT_DATE) - date_part('year', birthday))))
                BETWEEN CURRENT_DATE - 2 AND CURRENT_DATE
            )
            RETURNING user_id;
            """
        );

        var userIds = eligibleUsers
            .Select(u => u.user_id)
            .Where(IsNotNewAccount)
            .ToList();

        var rewardedUsers = await connection.QueryAsync<RewardedUserDto>(
            """
            UPDATE users.users SET taypoint_count = taypoint_count + @PointsToAdd
            WHERE user_id = ANY(@UserIds)
            RETURNING user_id, taypoint_count;
            """,
            new
            {
                PointsToAdd = rewardAmount,
                UserIds = userIds,
            }
        );

        transaction.Commit();

        return rewardedUsers.Select(
            u => new RewardedUser(new SnowflakeId(u.user_id), u.taypoint_count)
        ).ToList();
    }

    private bool IsNotNewAccount(string id)
    {
        var createdAt = SnowflakeUtils.FromSnowflake(new SnowflakeId(id));
        var timeSinceCreation = DateTimeOffset.UtcNow - createdAt;

        var isNewAccount = timeSinceCreation < TimeSpan.FromDays(7);
        if (isNewAccount)
        {
            logger.LogWarning("Excluding new account {UserId}, timeSinceCreation={TimeSinceCreation}.", id, timeSinceCreation);
        }

        return !isNewAccount;
    }
}
