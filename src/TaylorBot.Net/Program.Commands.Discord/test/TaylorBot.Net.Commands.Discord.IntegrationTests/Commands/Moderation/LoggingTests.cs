using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Moderation;

[Trait("Command", "mod log set")]
[Trait("Command", "mod log show")]
[Trait("Command", "mod spam add")]
[Trait("Command", "mod spam remove")]
[Trait("Command", "monitor members set")]
[Trait("Command", "monitor members show")]
[Trait("Command", "monitor deleted set")]
[Trait("Command", "monitor deleted show")]
[Trait("Command", "monitor edited set")]
[Trait("Command", "monitor edited show")]
public sealed class LoggingTests(DataServices data)
{
    [Theory]
    [InlineData("mod spam add")]
    [InlineData("mod spam remove")]
    public async Task SpamConfiguration_RequiresManageServerPermission(string route)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync();
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.MemberAsync(guild, user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, route, guild, permissions: "0");

        response.ShouldBeError();
        response.Description.Should().Contain("permission");
        (await scenario.State.SpamChannelAsync(guild)).Should().BeFalse();
    }

    [Theory]
    [InlineData("mod log", "mod", "Stop Logging")]
    [InlineData("monitor members", "members", "Stop Monitoring")]
    public async Task SetShowAndStop_PersistAndRemoveLogging(string route, string kind, string stopLabel)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.PlusAsync(user, activeGuilds: [guild]);
        scenario.DiscordApi.ExpectModerationLog(guild);

        var set = await scenario.Discord.InvokeSlashCommandAsync(user, $"{route} set", guild);
        var show = await scenario.Discord.InvokeSlashCommandAsync(user, $"{route} show", guild);

        set.ShouldBeSuccess();
        show.Description.Should().Contain(guild.ChannelId);
        (await scenario.State.LogChannelAsync(guild, kind)).Should().Be(guild.ChannelId);

        var stopped = await scenario.Discord.ClickAsync(user, show, stopLabel);

        stopped.ShouldBeSuccess();
        stopped.Description.Should().Contain("disabled");
        (await scenario.State.LogChannelAsync(guild, kind)).Should().BeNull();
    }

    [Theory]
    [InlineData("deleted")]
    [InlineData("edited")]
    public async Task MessageMonitoring_RequiresConsentBeforeSavingMessageContent(string kind)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.PlusAsync(user, activeGuilds: [guild]);

        var consent = await scenario.Discord.InvokeSlashCommandAsync(user, $"monitor {kind} set", guild);

        consent.Description.Should().Contain("save the content");
        (await scenario.State.LogChannelAsync(guild, kind)).Should().BeNull();

        var configured = await scenario.Discord.ClickAsync(user, consent, "Confirm");
        var show = await scenario.Discord.InvokeSlashCommandAsync(user, $"monitor {kind} show", guild);

        configured.ShouldBeSuccess();
        show.Description.Should().Contain(guild.ChannelId);
        (await scenario.State.LogChannelAsync(guild, kind)).Should().Be(guild.ChannelId);

        var stopped = await scenario.Discord.ClickAsync(user, show, "Stop Monitoring");

        stopped.ShouldBeSuccess();
        (await scenario.State.LogChannelAsync(guild, kind)).Should().BeNull();
    }

    [Theory]
    [InlineData("mod log", "mod")]
    [InlineData("monitor members", "members")]
    [InlineData("monitor deleted", "deleted")]
    [InlineData("monitor edited", "edited")]
    public async Task SetExisting_RequiresConfirmationBeforeReplacingChannel(string route, string kind)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.PlusAsync(user, activeGuilds: [guild]);
        await scenario.Given.LogChannelAsync(guild, kind, "100000000000000099");
        scenario.DiscordApi.ExpectModerationLog(guild);

        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, $"{route} set", guild);

        prompt.Description.Should().Contain("Are you sure");
        (await scenario.State.LogChannelAsync(guild, kind)).Should().Be("100000000000000099");

        var confirmed = await scenario.Discord.ClickAsync(user, prompt, "Confirm");

        confirmed.ShouldBeSuccess();
        (await scenario.State.LogChannelAsync(guild, kind)).Should().Be(guild.ChannelId);
    }

    [Theory]
    [InlineData("mod log", "mod")]
    [InlineData("monitor members", "members")]
    [InlineData("monitor deleted", "deleted")]
    [InlineData("monitor edited", "edited")]
    public async Task CancelReplacement_PreservesPreviousChannel(string route, string kind)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.PlusAsync(user, activeGuilds: [guild]);
        await scenario.Given.LogChannelAsync(guild, kind, "100000000000000099");

        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, $"{route} set", guild);
        var cancelled = await scenario.Discord.ClickAsync(user, prompt, "Cancel");

        cancelled.Description.Should().ContainEquivalentOf("cancel");
        (await scenario.State.LogChannelAsync(guild, kind)).Should().Be("100000000000000099");
    }

    [Theory]
    [InlineData("mod log show", "no moderation")]
    [InlineData("monitor members show", "not configured")]
    [InlineData("monitor deleted show", "not configured")]
    [InlineData("monitor edited show", "not configured")]
    public async Task ShowWithoutConfiguration_ExplainsHowToEnable(string route, string explanation)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, route, guild);

        response.Description.Should().Contain(explanation);
        response.Message.GetProperty("components").EnumerateArray().Should().BeEmpty();
    }

    [Theory]
    [InlineData("mod log show", "mod")]
    [InlineData("monitor members show", "members")]
    [InlineData("monitor deleted show", "deleted")]
    [InlineData("monitor edited show", "edited")]
    public async Task ShowDeletedChannel_ExplainsMissingChannelWithoutClearingConfiguration(string route, string kind)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, kind, "100000000000000099");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, route, guild);

        response.Description.Should().Contain("can't find").And.Contain("deleted");
        response.Message.GetProperty("components").EnumerateArray().Should().BeEmpty();
        (await scenario.State.LogChannelAsync(guild, kind)).Should().Be("100000000000000099");
    }

    [Theory]
    [InlineData("members")]
    [InlineData("deleted")]
    [InlineData("edited")]
    public async Task MonitorSet_RequiresPlusAndDoesNotPersist(string kind)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, $"monitor {kind} set", guild);

        response.ShouldBeError();
        response.Description.Should().Contain("Plus");
        (await scenario.State.LogChannelAsync(guild, kind)).Should().BeNull();
    }

    [Fact]
    public async Task SpamAddAndRemove_ToggleMessageCountingForCurrentChannel()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var added = await scenario.Discord.InvokeSlashCommandAsync(user, "mod spam add", guild);

        added.ShouldBeSuccess();
        added.Description.Should().Contain("no longer be counted");
        (await scenario.State.SpamChannelAsync(guild)).Should().BeTrue();

        var removed = await scenario.Discord.InvokeSlashCommandAsync(user, "mod spam remove", guild);

        removed.ShouldBeSuccess();
        removed.Description.Should().Contain("now counted");
        (await scenario.State.SpamChannelAsync(guild)).Should().BeFalse();
    }
}
