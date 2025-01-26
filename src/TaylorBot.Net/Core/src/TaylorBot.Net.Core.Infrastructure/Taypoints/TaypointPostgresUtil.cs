using Dapper;
using Npgsql;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.Infrastructure.Taypoints;

public record TaypointAddResult(long taypoint_count);

public record TaypointAddResults(string user_id, long taypoint_count);

public static class TaypointPostgresUtil
{
    public static async Task AddTaypointsAsync(NpgsqlConnection connection, SnowflakeId userId, long pointsToAdd)
    {
        await connection.ExecuteAsync(
            """
            UPDATE users.users
            SET taypoint_count = taypoint_count + @PointsToAdd
            WHERE user_id = @UserId;
            """,
            new
            {
                PointsToAdd = pointsToAdd,
                UserId = $"{userId}",
            }
        );
    }

    public static async Task<TaypointAddResult> AddTaypointsReturningAsync(NpgsqlConnection connection, SnowflakeId userId, long pointsToAdd)
    {
        return await connection.QuerySingleAsync<TaypointAddResult>(
            """
            UPDATE users.users
            SET taypoint_count = taypoint_count + @PointsToAdd
            WHERE user_id = @UserId
            RETURNING taypoint_count;
            """,
            new
            {
                PointsToAdd = pointsToAdd,
                UserId = $"{userId}",
            }
        );
    }

    public static async Task<IList<TaypointAddResults>> AddTaypointsForMultipleUsersAsync(NpgsqlConnection connection, long pointsToAdd, IReadOnlyList<SnowflakeId> userIds)
    {
        return (await connection.QueryAsync<TaypointAddResults>(
            """
            UPDATE users.users
            SET taypoint_count = taypoint_count + @PointsToAdd
            WHERE user_id = ANY(@UserIds)
            RETURNING user_id, taypoint_count;
            """,
            new
            {
                PointsToAdd = pointsToAdd,
                UserIds = userIds.Select(u => $"{u.Id}").ToList(),
            }
        )).ToList();
    }
}
