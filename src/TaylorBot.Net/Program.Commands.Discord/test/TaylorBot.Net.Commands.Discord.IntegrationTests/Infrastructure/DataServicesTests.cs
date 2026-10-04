using Dapper;
using FluentAssertions;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;

public sealed class DataServicesTests(DataServices services)
{
    [Fact]
    public async Task FreshSchema_HasApplicationRoleAndBalanceTable()
    {
        await using var database = await services.CreateDatabaseAsync();
        await using var connection = database.CreateConnection();

        var role = await connection.QuerySingleAsync<string>("SELECT current_user;");
        var isSuperuser = await connection.QuerySingleAsync<bool>("SELECT rolsuper FROM pg_roles WHERE rolname = current_user;");
        var users = await connection.QuerySingleAsync<long>("SELECT count(*) FROM users.users;");

        role.Should().Be("taylorbot");
        isSuperuser.Should().BeFalse();
        users.Should().Be(0);
        (await database.Redis.PingAsync()).Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public async Task SequentialConnections_ReuseBackendAndResetSession()
    {
        await using var database = await services.CreateDatabaseAsync();
        int backend;
        await using (var first = database.CreateConnection())
        {
            await first.OpenAsync(TestContext.Current.CancellationToken);
            backend = first.ProcessID;
            await first.ExecuteAsync("SET application_name = 'scenario-connection-probe';");
        }

        await using var second = database.CreateConnection();
        await second.OpenAsync(TestContext.Current.CancellationToken);

        second.ProcessID.Should().Be(backend);
        (await second.QuerySingleAsync<string>("SHOW application_name;")).Should().BeEmpty();
    }

    [Fact]
    public async Task NewScenario_DoesNotInheritDatabaseOrRedisChanges()
    {
        await using (var first = await services.CreateDatabaseAsync())
        {
            await using var connection = first.CreateConnection();
            await connection.ExecuteAsync("INSERT INTO users.users (user_id, username, is_bot) VALUES ('13', 'Alice', false);");
            await first.Redis.StringSetAsync("isolation-probe", "first scenario");
        }

        await using var second = await services.CreateDatabaseAsync();
        await using var freshConnection = second.CreateConnection();

        (await freshConnection.QuerySingleAsync<long>("SELECT count(*) FROM users.users;")).Should().Be(0);
        (await second.Redis.KeyExistsAsync("isolation-probe")).Should().BeFalse();
    }
}
