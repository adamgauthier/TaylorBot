using Dapper;
using Discord;
using Npgsql;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Rps.Infrastructure;

public class RpsStatsPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IRpsStatsRepository
{
    private static async Task AddRpsStatsAsync(NpgsqlConnection connection, IUser user, int winCount, int drawCount, int loseCount)
    {
        await connection.ExecuteAsync(
            $"""
            INSERT INTO users.rps_stats (user_id, rps_win_count, rps_draw_count, rps_lose_count)
            VALUES (@UserId, @WinCount, @DrawCount, @LoseCount)
            ON CONFLICT (user_id) DO UPDATE SET
                rps_win_count = rps_stats.rps_win_count + @WinCount,
                rps_draw_count = rps_stats.rps_draw_count + @DrawCount,
                rps_lose_count = rps_stats.rps_lose_count + @LoseCount
            ;
            """,
            new
            {
                UserId = $"{user.Id}",
                WinCount = winCount,
                DrawCount = drawCount,
                LoseCount = loseCount,
            }
        );
    }

    public async Task WinRpsAsync(IUser user, long taypointReward)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await AddRpsStatsAsync(connection, user, winCount: 1, 0, 0);

        await connection.ExecuteAsync(
            """
            UPDATE users.users
            SET taypoint_count = taypoint_count + @PointsToAdd
            WHERE user_id = @UserId
            RETURNING taypoint_count;
            """,
            new
            {
                PointsToAdd = taypointReward,
                UserId = $"{user.Id}",
            }
        );

        transaction.Commit();
    }

    public async Task DrawRpsAsync(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        await AddRpsStatsAsync(connection, user, 0, drawCount: 1, 0);
    }

    public async Task LoseRpsAsync(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        await AddRpsStatsAsync(connection, user, 0, 0, loseCount: 1);
    }

    public async Task<RpsProfile?> GetProfileAsync(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<RpsProfile?>(
            """
            SELECT rps_win_count, rps_draw_count, rps_lose_count
            FROM users.rps_stats
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }

    public async Task<IList<RpsLeaderboardEntry>> GetLeaderboardAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return (await connection.QueryAsync<RpsLeaderboardEntry>(
            """
            SELECT gm.user_id, u.username, rs.rps_win_count, rank() OVER (ORDER BY rps_win_count DESC) AS rank
            FROM guilds.guild_members AS gm
            JOIN users.rps_stats AS rs ON rs.user_id = gm.user_id
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
}
