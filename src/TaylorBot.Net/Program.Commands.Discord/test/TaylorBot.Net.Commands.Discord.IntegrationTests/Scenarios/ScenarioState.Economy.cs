using System.Text.Json;
using Dapper;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

public sealed record EconomyGameState(long Played, long Wins, long Draws, long Losses, long Won, long Lost);

public sealed partial class ScenarioState
{
    public Task<long> DailyStreakAsync(ScenarioUser user) =>
        ReadAsync<long>("SELECT streak_count FROM users.daily_payouts WHERE user_id = @Id;", new { user.Id });

    public Task<DateTime?> JoinedAsync(ScenarioGuild guild, ScenarioUser user) =>
        ReadAsync<DateTime?>("SELECT first_joined_at FROM guilds.guild_members WHERE guild_id = @GuildId AND user_id = @Id;", new { GuildId = guild.Id, user.Id });

    public async Task<EconomyGameState> GameAsync(ScenarioUser user, string game)
    {
        var sql = game switch
        {
            "risk" => "SELECT risk_win_count + risk_lose_count AS Played, risk_win_count AS Wins, 0::bigint AS Draws, risk_lose_count AS Losses, risk_win_amount AS Won, risk_lose_amount AS Lost FROM users.risk_stats WHERE user_id = @Id;",
            "heist" => "SELECT heist_win_count + heist_lose_count AS Played, heist_win_count AS Wins, 0::bigint AS Draws, heist_lose_count AS Losses, heist_win_amount AS Won, heist_lose_amount AS Lost FROM users.heist_stats WHERE user_id = @Id;",
            "roll" => "SELECT roll_count AS Played, perfect_roll_count AS Wins, 0::bigint AS Draws, 0::bigint AS Losses, 0::bigint AS Won, 0::bigint AS Lost FROM users.roll_stats WHERE user_id = @Id;",
            "rps" => "SELECT (rps_win_count + rps_draw_count + rps_lose_count)::bigint AS Played, rps_win_count::bigint AS Wins, rps_draw_count::bigint AS Draws, rps_lose_count::bigint AS Losses, 0::bigint AS Won, 0::bigint AS Lost FROM users.rps_stats WHERE user_id = @Id;",
            _ => throw new ArgumentOutOfRangeException(nameof(game)),
        };
        await using var connection = _database.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<EconomyGameState>(sql, new { user.Id })
            ?? new(Played: 0, Wins: 0, Draws: 0, Losses: 0, Won: 0, Lost: 0);
    }

    public async Task<long> HeistInvestmentAsync(ScenarioGuild guild, ScenarioUser user)
    {
        string? json = await _database.Redis.HashGetAsync($"heist:guild:{guild.Id}", user.Id);
        using var document = JsonDocument.Parse(json!);
        return document.RootElement.GetProperty("Absolute").GetProperty("Amount").GetInt64();
    }

    public async Task<bool> HasOpenHeistAsync(ScenarioGuild guild) =>
        await _database.Redis.KeyExistsAsync($"heist:guild:{guild.Id}");
}
