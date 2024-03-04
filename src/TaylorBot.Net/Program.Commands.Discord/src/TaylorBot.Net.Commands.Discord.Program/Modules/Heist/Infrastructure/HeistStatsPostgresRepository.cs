using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Infrastructure;

public class HeistStatsPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IHeistStatsRepository
{
    public async Task<List<HeistResult>> WinHeistAsync(IList<HeistPlayer> players, string payoutMultiplier)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        List<HeistResult> results = [];

        foreach (var player in players)
        {
            var transfer = await RiskPostgresUtil.WinRiskAsync(payoutMultiplier, connection, player.UserId, player.Amount);

            await connection.ExecuteAsync(
                """
                INSERT INTO users.heist_stats (user_id, heist_win_count, heist_win_amount)
                VALUES (@UserId, @WinCount, @WinAmount)
                ON CONFLICT (user_id) DO UPDATE SET
                    heist_win_count = heist_stats.heist_win_count + @WinCount,
                    heist_win_amount = heist_stats.heist_win_amount + @WinAmount;
                """,
                new
                {
                    player.UserId,
                    WinCount = 1,
                    WinAmount = transfer.profit_count,
                }
            );

            results.Add(new(player.UserId, transfer.invested_count, transfer.final_count, transfer.profit_count));
        }

        transaction.Commit();
        return results;
    }

    public async Task<List<HeistResult>> LoseHeistAsync(IList<HeistPlayer> players)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        List<HeistResult> results = [];

        foreach (var player in players)
        {
            var transfer = await RiskPostgresUtil.LoseRiskAsync(connection, player.UserId, player.Amount);

            await connection.ExecuteAsync(
                """
                INSERT INTO users.heist_stats (user_id, heist_lose_count, heist_lose_amount)
                VALUES (@UserId, @LoseCount, @LoseAmount)
                ON CONFLICT (user_id) DO UPDATE SET
                    heist_lose_count = heist_stats.heist_lose_count + @LoseCount,
                    heist_lose_amount = heist_stats.heist_lose_amount + @LoseAmount;
                """,
                new
                {
                    player.UserId,
                    LoseCount = 1,
                    LoseAmount = -transfer.profit_count,
                }
            );

            results.Add(new(player.UserId, transfer.invested_count, transfer.final_count, transfer.profit_count));
        }

        transaction.Commit();
        return results;
    }

    public async Task<HeistProfile?> GetProfileAsync(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<HeistProfile?>(
            """            
            SELECT heist_win_count, heist_win_amount, heist_lose_count, heist_lose_amount
            FROM users.heist_stats
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }

    public async Task<IList<HeistLeaderboardEntry>> GetLeaderboardAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return (await connection.QueryAsync<HeistLeaderboardEntry>(
            // Querying for users with wins first, expectation is the row count will be lower than the guild members count for large guilds
            // Then we join to filter out users that are not part of the guild and get the top 100
            // Finally we join on users to get their latest username
            """
            SELECT leaderboard.user_id, username, heist_win_count, rank FROM
            (
                SELECT heist_users.user_id, heist_win_count, rank() OVER (ORDER BY heist_win_count DESC) AS rank FROM
                (
                    SELECT user_id, heist_win_count
                    FROM users.heist_stats
                    WHERE heist_win_count > 0
                ) heist_users
                JOIN guilds.guild_members AS gm ON heist_users.user_id = gm.user_id AND gm.guild_id = @GuildId AND gm.alive = TRUE
                ORDER BY heist_win_count DESC
                LIMIT 100
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
