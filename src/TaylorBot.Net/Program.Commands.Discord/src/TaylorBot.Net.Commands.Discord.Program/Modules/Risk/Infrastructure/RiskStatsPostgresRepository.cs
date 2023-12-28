using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Risk.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Risk.Infrastructure;

public class RiskStatsPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IRiskStatsRepository
{
    public async Task<RiskResult> WinAsync(IUser user, ITaypointAmount amount)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var transfer = await RiskPostgresUtil.WinRiskAsync(payoutMultiplier: "1", connection, user.Id, amount);

        await connection.ExecuteAsync(
            """
            INSERT INTO users.gamble_stats (user_id, gamble_win_count, gamble_win_amount)
            VALUES (@UserId, @WinCount, @WinAmount)
            ON CONFLICT (user_id) DO UPDATE SET
                gamble_win_count = gamble_stats.gamble_win_count + @WinCount,
                gamble_win_amount = gamble_stats.gamble_win_amount + @WinAmount
            ;
            """,
            new
            {
                UserId = $"{user.Id}",
                WinCount = 1,
                WinAmount = transfer.profit_count,
            }
        );

        transaction.Commit();
        return new(transfer.invested_count, transfer.final_count, transfer.profit_count);
    }

    public async Task<RiskResult> LoseAsync(IUser user, ITaypointAmount amount)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var transfer = await RiskPostgresUtil.LoseRiskAsync(connection, user.Id, amount);

        await connection.ExecuteAsync(
            """
            INSERT INTO users.gamble_stats (user_id, gamble_lose_count, gamble_lose_amount)
            VALUES (@UserId, @LoseCount, @LoseAmount)
            ON CONFLICT (user_id) DO UPDATE SET
                gamble_lose_count = gamble_stats.gamble_lose_count + @LoseCount,
                gamble_lose_amount = gamble_stats.gamble_lose_amount + @LoseAmount
            ;
            """,
            new
            {
                UserId = $"{user.Id}",
                LoseCount = 1,
                LoseAmount = -transfer.profit_count,
            }
        );

        transaction.Commit();
        return new(transfer.invested_count, transfer.final_count, transfer.profit_count);
    }

    public async Task<RiskProfile?> GetProfileAsync(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<RiskProfile?>(
            """            
            SELECT gamble_win_count, gamble_win_amount, gamble_lose_count, gamble_lose_amount
            FROM users.gamble_stats
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }

    public async Task<IList<RiskLeaderboardEntry>> GetLeaderboardAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return (await connection.QueryAsync<RiskLeaderboardEntry>(
            """
            SELECT gm.user_id, u.username, gs.gamble_win_count, rank() OVER (ORDER BY gamble_win_count DESC) AS rank
            FROM guilds.guild_members AS gm
            JOIN users.gamble_stats AS gs ON gs.user_id = gm.user_id
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
