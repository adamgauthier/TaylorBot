using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Framework;

[Trait("Command", "taypoints balance")]
[Trait("Command", "owner ignore")]
public sealed class UserIgnoreTests(DataServices data)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NewUser_FirstSlashCommandSucceedsAndRecordsCreation(bool useRedisCache)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: CacheSettings(useRedisCache));
        var user = scenario.Given.UntrackedUser();
        var before = DateTimeOffset.UtcNow;

        var first = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");
        var created = await scenario.State.UserCreatedAtAsync(user);
        var second = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");

        first.ShouldBeSuccess();
        second.ShouldBeSuccess();
        (await scenario.State.IgnoredUntilAsync(user)).Should().BeNull();
        created.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTimeOffset.UtcNow);
        (await scenario.State.UserCreatedAtAsync(user)).Should().Be(created);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NewUser_FirstPrefixCommandSucceeds(bool useRedisCache)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: CacheSettings(useRedisCache));
        var owner = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(owner);
        var user = scenario.Given.UntrackedUser();

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!points");

        response.ShouldBeSuccess();
        (await scenario.State.IgnoredUntilAsync(user)).Should().BeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ActiveIgnore_PrefixRejectionRemainsSilent(bool useRedisCache)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: CacheSettings(useRedisCache));
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.IgnoreUntilAsync(user, DateTimeOffset.UtcNow.AddHours(2));

        var cold = await scenario.Discord.SendMessageAsync(user, guild, "!points", expectResponse: false);
        var warm = await scenario.Discord.SendMessageAsync(user, guild, "!points", expectResponse: false);

        cold.Requests.Should().BeEmpty();
        warm.Requests.Should().BeEmpty();
    }

    [Theory]
    [InlineData(false, -1)]
    [InlineData(false, 1)]
    [InlineData(true, -1)]
    [InlineData(true, 1)]
    public async Task ExistingRow_PreservesExpiredAndActiveIgnores(bool useRedisCache, int hours)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: CacheSettings(useRedisCache));
        var user = await scenario.Given.UserAsync();
        var until = DateTimeOffset.UtcNow.AddHours(hours);
        await scenario.Given.IgnoreUntilAsync(user, until);

        var cold = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");
        var warm = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");

        AssertIgnored(cold, expected: hours > 0);
        AssertIgnored(warm, expected: hours > 0);
        (await scenario.State.IgnoredUntilAsync(user)).Should().BeCloseTo(until, precision: TimeSpan.FromMilliseconds(1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public async Task LegacyCache_PreservesUnixMillisecondEntries(int hours)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var until = DateTimeOffset.UtcNow.AddHours(hours);
        await scenario.Given.CachedIgnoreUntilAsync(user, until);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");

        AssertIgnored(response, expected: hours > 0);
        (await scenario.State.CachedIgnoreUntilAsync(user)).Should().Be($"{until.ToUnixTimeMilliseconds()}");
    }

    [Fact]
    public async Task NullIgnore_IsCachedAsZeroRatherThanRepeatedDatabaseMisses()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = scenario.Given.UntrackedUser();

        var first = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");
        await scenario.Given.IgnoreUntilAsync(user, DateTimeOffset.UtcNow.AddHours(2));
        var cached = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");

        first.ShouldBeSuccess();
        cached.ShouldBeSuccess();
        (await scenario.State.CachedIgnoreUntilAsync(user)).Should().Be("0");
    }

    [Fact]
    public async Task NullIgnore_AfterCacheEvictionLoadsExistingRowAndRecachesZero()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = scenario.Given.UntrackedUser();

        var first = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");
        await scenario.Given.ClearIgnoreCacheAsync(user);
        var reloaded = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");

        first.ShouldBeSuccess();
        reloaded.ShouldBeSuccess();
        (await scenario.State.IgnoredUntilAsync(user)).Should().BeNull();
        (await scenario.State.CachedIgnoreUntilAsync(user)).Should().Be("0");
    }

    [Fact]
    public async Task LegacyEpochCache_IsANonIgnoredCacheHit()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.IgnoreUntilAsync(user, DateTimeOffset.UtcNow.AddHours(2));
        await scenario.Given.CachedIgnoreUntilAsync(user, DateTimeOffset.UnixEpoch);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");

        response.ShouldBeSuccess();
        (await scenario.State.CachedIgnoreUntilAsync(user)).Should().Be("0");
    }

    [Fact]
    public async Task OwnerIgnore_ReplacesCachedNullAndStillBlocksAfterCacheEviction()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var user = scenario.Given.UntrackedUser();

        var initial = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");
        var ignored = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner ignore",
            arguments: [SlashArgument.User("user", user), SlashArgument.Text("time", "2h")]);
        var cached = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");
        await scenario.Given.ClearIgnoreCacheAsync(user);
        var persisted = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");

        initial.ShouldBeSuccess();
        ignored.ShouldBeSuccess();
        cached.ShouldBeError();
        persisted.ShouldBeError();
        (await scenario.State.CachedIgnoreUntilAsync(user)).Should().NotBeNull().And.NotBe("0");
    }

    private static Dictionary<string, string?> CacheSettings(bool enabled) =>
        new() { ["CommandClient:UseRedisCache"] = $"{enabled}" };

    private static void AssertIgnored(DiscordExchange response, bool expected)
    {
        if (expected)
        {
            response.ShouldBeError();
            response.Description.Should().Contain("ignored until");
        }
        else
        {
            response.ShouldBeSuccess();
        }
    }
}
