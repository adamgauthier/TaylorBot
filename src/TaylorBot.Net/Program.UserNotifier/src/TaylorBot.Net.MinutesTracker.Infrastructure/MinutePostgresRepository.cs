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
            WITH active_members AS MATERIALIZED (
                SELECT guild_id, user_id FROM guilds.guild_members
                WHERE last_spoke_at IS NOT NULL
                AND last_spoke_at > CURRENT_TIMESTAMP - @MinimumTimeSpanSinceLastSpoke
                ORDER BY guild_id COLLATE "C", user_id COLLATE "C"
                FOR NO KEY UPDATE
            )
            UPDATE guilds.guild_members m
            SET minute_count = minute_count + 1
            FROM active_members a
            WHERE m.guild_id = a.guild_id AND m.user_id = a.user_id;
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
            WITH active_members AS MATERIALIZED (
                SELECT guild_id, user_id FROM guilds.guild_members
                WHERE last_spoke_at IS NOT NULL
                AND last_spoke_at > CURRENT_TIMESTAMP - @MinimumTimeSpanSinceLastSpoke
                ORDER BY guild_id COLLATE "C", user_id COLLATE "C"
                FOR NO KEY UPDATE
            ), active_users AS (
                UPDATE guilds.guild_members m
                SET
                    minute_count = minute_count + 1,
                    experience = experience + 1
                FROM active_members a
                WHERE m.guild_id = a.guild_id AND m.user_id = a.user_id
                RETURNING m.user_id
            ), locked_users AS MATERIALIZED (
                SELECT u.user_id FROM users.users u
                WHERE u.user_id IN (SELECT user_id FROM active_users)
                ORDER BY u.user_id COLLATE "C"
                FOR NO KEY UPDATE OF u
            )
            UPDATE users.users u
            SET taypoint_count = taypoint_count + 1
            FROM locked_users l
            WHERE u.user_id = l.user_id;
            """,
            new
            {
                MinimumTimeSpanSinceLastSpoke = minimumTimeSpanSinceLastSpoke,
            },
            commandTimeout: (int)TimeSpan.FromSeconds(45).TotalSeconds
        );
    }
}
