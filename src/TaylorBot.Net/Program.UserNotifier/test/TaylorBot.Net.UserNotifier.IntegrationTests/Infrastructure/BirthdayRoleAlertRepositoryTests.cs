using StackExchange.Redis;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.BirthdayReward.Infrastructure;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Infrastructure;

public sealed class BirthdayRoleAlertRepositoryTests(DataServices data)
{
    private static readonly SnowflakeId GuildId = new("100000000000000101");

    [Fact]
    public async Task IndependentConnections_ShareAtomicCooldown()
    {
        await using var database = await data.CreateDatabaseAsync();
        await using var firstConnection = await ConnectionMultiplexer.ConnectAsync($"{data.RedisHost}:{data.RedisPort}");
        await using var secondConnection = await ConnectionMultiplexer.ConnectAsync($"{data.RedisHost}:{data.RedisPort}");
        BirthdayRoleAlertRedisRepository first = new(firstConnection);
        BirthdayRoleAlertRedisRepository second = new(secondConnection);

        var claims = await Task.WhenAll(
            first.TryReserveAsync(GuildId, BirthdayRoleOperation.Assign, "first"),
            second.TryReserveAsync(GuildId, BirthdayRoleOperation.Assign, "second"));
        await second.MarkDeliveredAsync(GuildId, BirthdayRoleOperation.Assign, claims[0] ? "first" : "second");
        await firstConnection.DisposeAsync();
        var afterReconnect = await second.TryReserveAsync(GuildId, BirthdayRoleOperation.Assign, "later");

        claims.Should().ContainSingle(claimed => claimed);
        afterReconnect.Should().BeFalse();
        (await second.TryReserveAsync(GuildId, BirthdayRoleOperation.Remove, "removal")).Should().BeTrue();
        (await database.Redis.KeyTimeToLiveAsync($"birthday-role-alert:guild:{GuildId}:Assign"))!.Value
            .Should().BeCloseTo(TimeSpan.FromHours(24), precision: TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task ExpiredReservation_CannotExtendAnotherSenderClaim()
    {
        await using var database = await data.CreateDatabaseAsync();
        await using var connection = await ConnectionMultiplexer.ConnectAsync($"{data.RedisHost}:{data.RedisPort}");
        BirthdayRoleAlertRedisRepository repository = new(connection);
        var key = $"birthday-role-alert:guild:{GuildId}:Assign";
        (await repository.TryReserveAsync(GuildId, BirthdayRoleOperation.Assign, "expired")).Should().BeTrue();
        (await database.Redis.KeyExpireAsync(key, TimeSpan.Zero)).Should().BeTrue();
        (await repository.TryReserveAsync(GuildId, BirthdayRoleOperation.Assign, "current")).Should().BeTrue();

        var staleCompletion = () => repository.MarkDeliveredAsync(GuildId, BirthdayRoleOperation.Assign, "expired");

        await staleCompletion.Should().ThrowAsync<InvalidOperationException>();
        (await database.Redis.StringGetAsync(key)).ToString().Should().Be("current");
        (await database.Redis.KeyTimeToLiveAsync(key))!.Value
            .Should().BeCloseTo(TimeSpan.FromMinutes(5), precision: TimeSpan.FromSeconds(30));
    }
}
