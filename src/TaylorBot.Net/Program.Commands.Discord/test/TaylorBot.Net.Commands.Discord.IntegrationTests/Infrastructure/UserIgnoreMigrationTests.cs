using Dapper;
using FluentAssertions;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;

public sealed class UserIgnoreMigrationTests(DataServices services)
{
    [Fact]
    public async Task Deploy_PreservesLegacyRowsAndCachesAndBackfillsCreationTime()
    {
        await using var database = await services.CreateDatabaseAsync();
        await ArrangeLegacySchemaAsync(database);
        var users = await UsersAsync(database);
        var caches = await CachedIgnoresAsync(database);
        var before = DateTime.UtcNow;

        await services.RunSqitchAsync(database, "deploy", "--verify");
        await services.RunSqitchAsync(database, "verify", "--from-change", "20261012_user_ignore_state");

        (await UsersAsync(database)).Should().BeEquivalentTo(users);
        (await CachedIgnoresAsync(database)).Should().Equal(caches);
        (await CreationTimesAsync(database)).Should().OnlyContain(created => created >= before && created <= DateTime.UtcNow);
    }

    [Fact]
    public async Task Revert_PreservesExistingIgnoresAndSafelyConvertsNullBeforeRedeploy()
    {
        await using var database = await services.CreateDatabaseAsync();
        await ArrangeLegacySchemaAsync(database);
        await services.RunSqitchAsync(database, "deploy", "--verify");
        await InsertUserAsync(database, id: "new-user");
        var users = await UsersAsync(database);
        users.Should().ContainSingle(user => user.UserId == "new-user" && user.IgnoreUntil == null);

        await RevertAsync(database);

        (await UsersAsync(database)).Should().BeEquivalentTo(users.Select(user =>
            user with { IgnoreUntil = user.IgnoreUntil ?? DateTime.UnixEpoch }));
        (await LegacySchemaAsync(database)).Should().BeTrue();

        await services.RunSqitchAsync(database, "deploy", "--verify");
        await InsertUserAsync(database, id: "redeployed-user");

        (await UsersAsync(database)).Should().ContainSingle(user => user.UserId == "redeployed-user" && user.IgnoreUntil == null);
        (await UsersAsync(database)).Should().ContainSingle(user => user.UserId == "new-user" && user.IgnoreUntil == DateTime.UnixEpoch);
    }

    private Task RevertAsync(ScenarioDatabase database) =>
        services.RunSqitchAsync(database, "revert", "--to-change", "20260409_attributes_set_at", "-y");

    private async Task ArrangeLegacySchemaAsync(ScenarioDatabase database)
    {
        await RevertAsync(database);
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO users.users (user_id, username, previous_username, is_bot, taypoint_count, ignore_until)
            VALUES
                ('expired-user', 'Alice', 'Old Alice', false, 13, '2000-01-01T00:00:00Z'),
                ('active-user', 'Bot', NULL, true, 42, CURRENT_TIMESTAMP + interval '1 day');
            """);
        var ignores = await connection.QueryAsync<CachedIgnore>("SELECT user_id AS UserId, ignore_until AS Until FROM users.users;");
        foreach (var ignore in ignores)
        {
            await database.Redis.StringSetAsync($"ignore-until:user:{ignore.UserId}",
                new DateTimeOffset(ignore.Until).ToUnixTimeMilliseconds(), TimeSpan.FromHours(1));
        }
    }

    private static async Task<IReadOnlyList<UserRow>> UsersAsync(ScenarioDatabase database)
    {
        await using var connection = database.CreateConnection();
        return (await connection.QueryAsync<UserRow>("""
            SELECT user_id AS UserId, username AS Username, previous_username AS PreviousUsername,
                is_bot AS IsBot, taypoint_count AS Taypoints, ignore_until AS IgnoreUntil
            FROM users.users ORDER BY user_id;
            """)).AsList();
    }

    private static async Task<string?[]> CachedIgnoresAsync(ScenarioDatabase database) =>
        [
            await database.Redis.StringGetAsync("ignore-until:user:active-user"),
            await database.Redis.StringGetAsync("ignore-until:user:expired-user"),
        ];

    private static async Task<IEnumerable<DateTime>> CreationTimesAsync(ScenarioDatabase database)
    {
        await using var connection = database.CreateConnection();
        return await connection.QueryAsync<DateTime>("SELECT created_at FROM users.users;");
    }

    private static async Task InsertUserAsync(ScenarioDatabase database, string id)
    {
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync("INSERT INTO users.users (user_id, username, is_bot) VALUES (@id, 'New user', false);", new { id });
    }

    private static async Task<bool> LegacySchemaAsync(ScenarioDatabase database)
    {
        await using var connection = database.CreateConnection();
        return await connection.QuerySingleAsync<bool>("""
            SELECT EXISTS (
                SELECT FROM information_schema.columns
                WHERE table_schema = 'users' AND table_name = 'users' AND column_name = 'ignore_until'
                    AND is_nullable = 'NO' AND column_default = 'now()'
            ) AND NOT EXISTS (
                SELECT FROM information_schema.columns
                WHERE table_schema = 'users' AND table_name = 'users' AND column_name = 'created_at'
            );
            """);
    }

    private sealed record UserRow(string UserId, string Username, string? PreviousUsername, bool IsBot, long Taypoints, DateTime? IgnoreUntil);
    private sealed record CachedIgnore(string UserId, DateTime Until);
}
