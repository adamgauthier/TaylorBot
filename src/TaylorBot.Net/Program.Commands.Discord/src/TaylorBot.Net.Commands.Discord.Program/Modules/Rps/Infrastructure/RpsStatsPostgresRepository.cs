using Dapper;
using Npgsql;
using TaylorBot.Net.Commands.Discord.Program.Modules.Rps.Commands;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Taypoints;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Rps.Infrastructure;

public class RpsStatsPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IRpsStatsRepository
{
    private static async Task AddRpsStatsAsync(NpgsqlConnection connection, DiscordUser user, int winCount, int drawCount, int loseCount)
    {
        await connection.ExecuteAsync(
            """
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

    public async Task WinRpsAsync(DiscordUser user, long taypointReward)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await AddRpsStatsAsync(connection, user, winCount: 1, 0, 0);

        await TaypointPostgresUtil.AddTaypointsAsync(connection, user.Id, taypointReward);

        transaction.Commit();
    }

    public async Task DrawRpsAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        await AddRpsStatsAsync(connection, user, 0, drawCount: 1, 0);
    }

    public async Task LoseRpsAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        await AddRpsStatsAsync(connection, user, 0, 0, loseCount: 1);
    }

    public async Task<RpsProfile?> GetProfileAsync(DiscordUser user)
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

    public async Task<IList<RpsLeaderboardEntry>> GetLeaderboardAsync(CommandGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return (await connection.QueryAsync<RpsLeaderboardEntry>(
            // Querying for users with wins first, expectation is the row count will be lower than the guild members count for large guilds
            // Then we join to filter out users that are not part of the guild and get the top 100
            // Finally we join on users to get their latest username
            """
            SELECT leaderboard.user_id, username, rps_win_count, rank FROM
            (
                SELECT rps_users.user_id, rps_win_count, rank() OVER (ORDER BY rps_win_count DESC) AS rank FROM
                (
                    SELECT user_id, rps_win_count
                    FROM users.rps_stats
                    WHERE rps_win_count > 0
                ) rps_users
                JOIN guilds.guild_members AS gm ON rps_users.user_id = gm.user_id AND gm.guild_id = @GuildId AND gm.alive = TRUE
                ORDER BY rps_win_count DESC
                LIMIT 150
            ) leaderboard
            JOIN users.users AS u ON leaderboard.user_id = u.user_id;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        )).ToList();
    }
}
