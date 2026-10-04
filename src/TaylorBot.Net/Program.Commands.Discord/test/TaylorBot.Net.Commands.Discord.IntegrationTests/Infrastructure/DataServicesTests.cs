using Dapper;
using FluentAssertions;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: AssemblyFixture(typeof(TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure.DataServices))]

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
