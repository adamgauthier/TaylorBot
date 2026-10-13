using Dapper;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

public sealed partial class ScenarioData
{
    public async Task IgnoreUntilAsync(ScenarioUser user, DateTimeOffset? until)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("UPDATE users.users SET ignore_until = @until WHERE user_id = @Id;", new { user.Id, until });
    }

    public Task CachedIgnoreUntilAsync(ScenarioUser user, DateTimeOffset until) =>
        _database.Redis.StringSetAsync($"ignore-until:user:{user.Id}", until.ToUnixTimeMilliseconds(), TimeSpan.FromHours(1));

    public Task ClearIgnoreCacheAsync(ScenarioUser user) =>
        _database.Redis.KeyDeleteAsync($"ignore-until:user:{user.Id}");
}
