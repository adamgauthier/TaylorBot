using Dapper;
using Discord;
using Npgsql;
using TaylorBot.Net.Commands.Discord.Program.Modules.Roll.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Roll.Infrastructure;

public class RollStatsPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IRollStatsRepository
{
    public async Task<RollProfile?> GetProfileAsync(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<RollProfile?>(
            """
            SELECT roll_count, perfect_roll_count
            FROM users.roll_stats
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }

    public async Task<IList<RollLeaderboardEntry>> GetLeaderboardAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return (await connection.QueryAsync<RollLeaderboardEntry>(
            """
            SELECT gm.user_id, u.username, rs.perfect_roll_count, rank() OVER (ORDER BY perfect_roll_count DESC) AS rank
            FROM guilds.guild_members AS gm
            JOIN users.roll_stats AS rs ON rs.user_id = gm.user_id
            JOIN users.users AS u ON u.user_id = gm.user_id
            WHERE gm.guild_id = @GuildId AND gm.alive = TRUE AND u.is_bot = FALSE
            LIMIT 100;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        )).ToList();
    }

    public async Task WinRollAsync(IUser user, long taypointReward)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await AddRollCountAsync(connection, user);

        await TaypointPostgresUtil.AddTaypointsAsync(connection, user.Id, taypointReward);

        transaction.Commit();
    }

    public async Task WinPerfectRollAsync(IUser user, long taypointReward)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            """
            INSERT INTO users.roll_stats (user_id, roll_count, perfect_roll_count)
            VALUES (@UserId, @RollCount, @RollCount)
            ON CONFLICT (user_id) DO UPDATE SET
                roll_count = roll_stats.roll_count + @RollCount,
                perfect_roll_count = roll_stats.perfect_roll_count + @RollCount;
            """,
            new
            {
                UserId = $"{user.Id}",
                RollCount = 1,
            }
        );

        await TaypointPostgresUtil.AddTaypointsAsync(connection, user.Id, taypointReward);

        transaction.Commit();
    }

    public async Task AddRollCountAsync(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await AddRollCountAsync(connection, user);
    }

    private static async Task AddRollCountAsync(NpgsqlConnection connection, IUser user)
    {
        await connection.ExecuteAsync(
            """
            INSERT INTO users.roll_stats (user_id, roll_count)
            VALUES (@UserId, @RollCount)
            ON CONFLICT (user_id) DO UPDATE SET
                roll_count = roll_stats.roll_count + @RollCount;
            """,
            new
            {
                UserId = $"{user.Id}",
                RollCount = 1,
            }
        );
    }
}
