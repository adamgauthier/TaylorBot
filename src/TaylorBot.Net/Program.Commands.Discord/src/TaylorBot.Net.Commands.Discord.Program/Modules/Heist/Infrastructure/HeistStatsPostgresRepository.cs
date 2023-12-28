using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Infrastructure;

public class HeistStatsPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IHeistStatsRepository
{
    public record TaypointTransferDto(long invested_count, long final_count, long profit_count);

    private record TaypointUpdateInfo(string Query, long AmountParam);

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
            """
            SELECT gm.user_id, u.username, hs.heist_win_count, rank() OVER (ORDER BY heist_win_count DESC) AS rank
            FROM guilds.guild_members AS gm
            JOIN users.heist_stats AS hs ON hs.user_id = gm.user_id
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
