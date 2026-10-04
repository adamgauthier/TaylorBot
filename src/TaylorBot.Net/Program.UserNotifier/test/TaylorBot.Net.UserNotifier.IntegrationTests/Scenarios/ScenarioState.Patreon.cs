using Dapper;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed record PatronState(bool Active, int MaximumGuilds, string? RewardedCharge);

public sealed partial class ScenarioState
{
    public async Task<PatronState?> PatronAsync(ScenarioUser user)
    {
        await using var connection = database.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<PatronState>(
            "SELECT active AS Active, max_plus_guilds AS MaximumGuilds, rewarded_for_charge_at AS RewardedCharge FROM plus.plus_users WHERE user_id = @Id;", new { user.Id });
    }

    public async Task<long> PatronCountAsync()
    {
        await using var connection = database.CreateConnection();
        return await connection.QuerySingleAsync<long>("SELECT count(*) FROM plus.plus_users;");
    }

    public async Task<string> PlusGuildStateAsync(ScenarioGuild guild)
    {
        await using var connection = database.CreateConnection();
        return await connection.QuerySingleAsync<string>("SELECT state FROM plus.plus_guilds WHERE guild_id = @Id;", new { guild.Id });
    }
}
