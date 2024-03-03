using Dapper;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.MinutesTracker.Domain;

namespace TaylorBot.Net.MinutesTracker.Infrastructure;

public class MinutePostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IMinuteRepository
{
    public async Task AddMinuteToActiveUsersAsync(TimeSpan minimumTimeSpanSinceLastSpoke)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE guilds.guild_members
            SET minute_count = minute_count + 1
            WHERE last_spoke_at IS NOT NULL
            AND last_spoke_at > CURRENT_TIMESTAMP - @MinimumTimeSpanSinceLastSpoke;
            """,
            new
            {
                MinimumTimeSpanSinceLastSpoke = minimumTimeSpanSinceLastSpoke,
            },
            commandTimeout: (int)TimeSpan.FromSeconds(45).TotalSeconds
        );
    }

    public async Task AddMinuteAndPointToActiveUsersAsync(TimeSpan minimumTimeSpanSinceLastSpoke)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            WITH active_users AS (
                UPDATE guilds.guild_members
                SET
                    minute_count = minute_count + 1,
                    experience = experience + 1
                WHERE last_spoke_at IS NOT NULL
                AND last_spoke_at > CURRENT_TIMESTAMP - @MinimumTimeSpanSinceLastSpoke
                RETURNING user_id
            )
            UPDATE users.users
            SET taypoint_count = taypoint_count + 1
            WHERE user_id IN (SELECT DISTINCT(user_id) FROM active_users);
            """,
            new
            {
                MinimumTimeSpanSinceLastSpoke = minimumTimeSpanSinceLastSpoke,
            },
            commandTimeout: (int)TimeSpan.FromSeconds(45).TotalSeconds
        );
    }
}
