using Dapper;
using TaylorBot.Net.Commands.Discord.Program.Modules.Risk.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Risk.Infrastructure;

public class RiskStatsPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IRiskStatsRepository
{
    public async Task<RiskResult> WinAsync(DiscordUser user, ITaypointAmount amount, RiskLevel level)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        var payoutMultiplier = level switch
        {
            RiskLevel.Low => "1",
            RiskLevel.Moderate => "3",
            RiskLevel.High => "9",
            _ => throw new NotImplementedException(),
        };

        var transfer = await RiskPostgresUtil.WinRiskAsync(payoutMultiplier, connection, user.Id, amount);

        await connection.ExecuteAsync(
            """
            INSERT INTO users.risk_stats (user_id, risk_win_count, risk_win_amount)
            VALUES (@UserId, @WinCount, @WinAmount)
            ON CONFLICT (user_id) DO UPDATE SET
                risk_win_count = risk_stats.risk_win_count + @WinCount,
                risk_win_amount = risk_stats.risk_win_amount + @WinAmount
            ;
            """,
            new
            {
                UserId = $"{user.Id}",
                WinCount = 1,
                WinAmount = transfer.profit_count,
            }
        );

        await transaction.CommitAsync();
        return new(transfer.invested_count, transfer.final_count, transfer.profit_count);
    }

    public async Task<RiskResult> LoseAsync(DiscordUser user, ITaypointAmount amount)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        var transfer = await RiskPostgresUtil.LoseRiskAsync(connection, user.Id, amount);

        await connection.ExecuteAsync(
            """
            INSERT INTO users.risk_stats (user_id, risk_lose_count, risk_lose_amount)
            VALUES (@UserId, @LoseCount, @LoseAmount)
            ON CONFLICT (user_id) DO UPDATE SET
                risk_lose_count = risk_stats.risk_lose_count + @LoseCount,
                risk_lose_amount = risk_stats.risk_lose_amount + @LoseAmount
            ;
            """,
            new
            {
                UserId = $"{user.Id}",
                LoseCount = 1,
                LoseAmount = -transfer.profit_count,
            }
        );

        await transaction.CommitAsync();
        return new(transfer.invested_count, transfer.final_count, transfer.profit_count);
    }

    public async Task<RiskProfile?> GetProfileAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<RiskProfile?>(
            """
            SELECT risk_win_count, risk_win_amount, risk_lose_count, risk_lose_amount
            FROM users.risk_stats
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }

    public async Task<IList<RiskLeaderboardEntry>> GetLeaderboardAsync(CommandGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return [.. (await connection.QueryAsync<RiskLeaderboardEntry>(
            // Querying for users with wins first, expectation is the row count will be lower than the guild members count for large guilds
            // Then we join to filter out users that are not part of the guild and get the top 100
            // Finally we join on users to get their latest username
            """
            SELECT leaderboard.user_id, username, risk_win_count, rank FROM
            (
                SELECT risk_users.user_id, risk_win_count, rank() OVER (ORDER BY risk_win_count DESC) AS rank FROM
                (
                    SELECT user_id, risk_win_count
                    FROM users.risk_stats
                    WHERE risk_win_count > 0
                ) risk_users
                JOIN guilds.guild_members AS gm ON risk_users.user_id = gm.user_id AND gm.guild_id = @GuildId AND gm.alive = TRUE
                ORDER BY risk_win_count DESC
                LIMIT 150
            ) leaderboard
            JOIN users.users AS u ON leaderboard.user_id = u.user_id;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        ))];
    }
}
