using Dapper;
using Npgsql;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;

public static class TaypointPostgresUtil
{
    public static async Task AddTaypointsAsync(NpgsqlConnection connection, SnowflakeId userId, long pointsToAdd)
    {
        await connection.ExecuteAsync(
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
