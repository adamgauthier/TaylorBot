using Dapper;
using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Taypoints;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.BirthdayReward.Infrastructure;

public class BirthdayPostgresRepository(ILogger<BirthdayPostgresRepository> logger, PostgresConnectionFactory postgresConnectionFactory) : IBirthdayRepository
{
    private record EligibleUserDto(string user_id);

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
            .Select(u => new SnowflakeId(u.user_id))
            .Where(IsNotNewAccount)
            .ToList();

        var rewardedUsers = await TaypointPostgresUtil.AddTaypointsForMultipleUsersAsync(connection, rewardAmount, userIds);

        transaction.Commit();

        return rewardedUsers.Select(
            u => new RewardedUser(new SnowflakeId(u.user_id), u.taypoint_count)
        ).ToList();
    }

    private bool IsNotNewAccount(SnowflakeId id)
    {
        var createdAt = SnowflakeUtils.FromSnowflake(id);
        var timeSinceCreation = DateTimeOffset.UtcNow - createdAt;

        var isNewAccount = timeSinceCreation < TimeSpan.FromDays(7);
        if (isNewAccount)
        {
            logger.LogWarning("Excluding new account {UserId}, timeSinceCreation={TimeSinceCreation}.", id, timeSinceCreation);
        }

        return !isNewAccount;
    }
}
