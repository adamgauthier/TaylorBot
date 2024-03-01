using Dapper;
using Npgsql;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.MinutesTracker.Domain;

namespace TaylorBot.Net.MinutesTracker.Infrastructure;

public class MinutesRepository(PostgresConnectionFactory postgresConnectionFactory) : IMinuteRepository
{
    public async ValueTask AddMinutesToActiveMembersAsync(long minutesToAdd, TimeSpan minimumTimeSpanSinceLastSpoke, long minutesRequiredForReward, long pointsReward)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();

        await connection.ExecuteAsync(
            """
            UPDATE guilds.guild_members
            SET minute_count = minute_count + @MinutesToAdd
            WHERE last_spoke_at > CURRENT_TIMESTAMP - @MinimumTimeSpanSinceLastSpoke;
            """,
            new
            {
                MinutesToAdd = minutesToAdd,
                MinimumTimeSpanSinceLastSpoke = minimumTimeSpanSinceLastSpoke
            },
            commandTimeout: (int)TimeSpan.FromSeconds(45).TotalSeconds
        );

        await AddTaypointsAsync(minutesRequiredForReward, pointsReward, connection);
    }

    private static async Task AddTaypointsAsync(long minutesRequiredForReward, long pointsReward, NpgsqlConnection connection)
    {
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            """
            UPDATE users.users SET
                taypoint_count = taypoint_count + @PointsReward
            WHERE user_id IN (
                SELECT user_id FROM guilds.guild_members
                WHERE minute_count >= minutes_milestone + @MinutesRequiredForReward
            );
            """,
            new
            {
                PointsReward = pointsReward,
                MinutesRequiredForReward = minutesRequiredForReward
            }
        );

        await connection.ExecuteAsync(
            """
            UPDATE guilds.guild_members SET
                minutes_milestone = (minute_count - (minute_count % @MinutesRequiredForReward)),
                experience = experience + @PointsReward
            WHERE minute_count >= minutes_milestone + @MinutesRequiredForReward;
            """,
            new
            {
                PointsReward = pointsReward,
                MinutesRequiredForReward = minutesRequiredForReward
            }
        );

        transaction.Commit();
    }
}
