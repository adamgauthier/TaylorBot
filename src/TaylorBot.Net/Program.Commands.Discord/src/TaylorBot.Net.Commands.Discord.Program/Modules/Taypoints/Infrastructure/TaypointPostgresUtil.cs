using Dapper;
using Npgsql;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;

public record TaypointAddResult(long taypoint_count);

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
}
