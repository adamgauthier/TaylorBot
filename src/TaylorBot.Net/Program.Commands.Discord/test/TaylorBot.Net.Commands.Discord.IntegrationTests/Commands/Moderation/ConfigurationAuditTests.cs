using FluentAssertions;
using System.Net;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Moderation;

public sealed class ConfigurationAuditTests(DataServices data)
{
    [Fact]
    public async Task ModLog_SetMoveAndDisable_LogsToAffectedChannels()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        var nextChannel = await scenario.Given.TextChannelAsync(guild, "new-mod-log");
        scenario.DiscordApi.ExpectModerationLog(guild);
        scenario.DiscordApi.ExpectModerationLog(nextChannel);

        var initial = await scenario.Discord.InvokeSlashCommandAsync(moderator, "mod log set", guild);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(moderator, "mod log set", guild,
            arguments: [SlashArgument.Channel("channel", nextChannel)]);

        initial.ShouldBeSuccess();
        scenario.DiscordApi.ShouldHaveConfigurationChanges(guild, moderator,
            ("Moderation log channel", "Not configured", $"<#{guild.ChannelId}>"));
        scenario.DiscordApi.ModerationLogs(nextChannel).Should().BeEmpty();

        var moved = await scenario.Discord.ClickAsync(moderator, prompt, "Confirm");
        var settings = await scenario.Discord.InvokeSlashCommandAsync(moderator, "mod log show", guild);
        var disabled = await scenario.Discord.ClickAsync(moderator, settings, "Stop Logging");

        moved.ShouldBeSuccess();
        disabled.ShouldBeSuccess();
        (await scenario.State.LogChannelAsync(guild, "mod")).Should().BeNull();
        scenario.DiscordApi.ShouldHaveConfigurationChanges(guild, moderator,
            ("Moderation log channel", "Not configured", $"<#{guild.ChannelId}>"),
            ("Moderation log channel", $"<#{guild.ChannelId}>", $"<#{nextChannel.ChannelId}>"));
        scenario.DiscordApi.ShouldHaveConfigurationChanges(nextChannel, moderator,
            ("Moderation log channel", $"<#{guild.ChannelId}>", $"<#{nextChannel.ChannelId}>"),
            ("Moderation log channel", $"<#{nextChannel.ChannelId}>", "Not configured"));
    }

    [Theory]
    [InlineData("members", "Member monitoring")]
    [InlineData("deleted", "Deleted message monitoring")]
    [InlineData("edited", "Edited message monitoring")]
    public async Task Monitoring_EnableMoveAndDisable_LogsEachSavedChange(string kind, string setting)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        var nextChannel = await scenario.Given.TextChannelAsync(guild, "monitor");
        await scenario.Given.PlusAsync(moderator, activeGuilds: [guild]);
        await scenario.Given.ModerationLogAsync(guild);

        await EnableMonitoringAsync(scenario, moderator, guild, kind);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(moderator, $"monitor {kind} set", guild,
            arguments: [SlashArgument.Channel("channel", nextChannel)]);
        var moved = await scenario.Discord.ClickAsync(moderator, prompt, "Confirm");
        var settings = await scenario.Discord.InvokeSlashCommandAsync(moderator, $"monitor {kind} show", guild);
        var disabled = await scenario.Discord.ClickAsync(moderator, settings, "Stop Monitoring");

        moved.ShouldBeSuccess();
        disabled.ShouldBeSuccess();
        (await scenario.State.LogChannelAsync(guild, kind)).Should().BeNull();
        scenario.DiscordApi.ShouldHaveConfigurationChanges(guild, moderator,
            (setting, "Not configured", $"<#{guild.ChannelId}>"),
            (setting, $"<#{guild.ChannelId}>", $"<#{nextChannel.ChannelId}>"),
            (setting, $"<#{nextChannel.ChannelId}>", "Not configured"));
    }

    [Theory]
    [InlineData("deleted")]
    [InlineData("edited")]
    public async Task Monitoring_CancelConsent_DoesNotLogOrConfigure(string kind)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.PlusAsync(moderator, activeGuilds: [guild]);
        await scenario.Given.ModerationLogAsync(guild);

        var prompt = await scenario.Discord.InvokeSlashCommandAsync(moderator, $"monitor {kind} set", guild);
        await scenario.Discord.ClickAsync(moderator, prompt, "Cancel");

        (await scenario.State.LogChannelAsync(guild, kind)).Should().BeNull();
        scenario.DiscordApi.ModerationLogs(guild).Should().BeEmpty();
    }

    [Fact]
    public async Task Modmail_EnableMoveAndDisable_LogsOnlyConfirmedChanges()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        var nextChannel = await scenario.Given.TextChannelAsync(guild, "modmail");
        await scenario.Given.ModerationLogAsync(guild);

        var settings = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail config", guild);
        await scenario.Discord.SelectChannelAsync(moderator, settings, guild);
        var configured = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail config", guild);
        var prompt = await scenario.Discord.SelectChannelAsync(moderator, configured, nextChannel);

        scenario.DiscordApi.ShouldHaveConfigurationChanges(guild, moderator,
            ("Mod Mail channel", "Not configured", $"<#{guild.ChannelId}>"));

        await scenario.Discord.ClickAsync(moderator, prompt, "Confirm");
        var updated = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail config", guild);
        await scenario.Discord.ClickAsync(moderator, updated, "Disable Mod Mail");

        (await scenario.State.LogChannelAsync(guild, "modmail")).Should().BeNull();
        scenario.DiscordApi.ShouldHaveConfigurationChanges(guild, moderator,
            ("Mod Mail channel", "Not configured", $"<#{guild.ChannelId}>"),
            ("Mod Mail channel", $"<#{guild.ChannelId}>", $"<#{nextChannel.ChannelId}>"),
            ("Mod Mail channel", $"<#{nextChannel.ChannelId}>", "Not configured"));
    }

    [Fact]
    public async Task SpamChannel_RepeatedChangesOnlyLogActualTransitions()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.ModerationLogAsync(guild);

        await scenario.Discord.InvokeSlashCommandAsync(moderator, "mod spam add", guild);
        await scenario.Discord.InvokeSlashCommandAsync(moderator, "mod spam add", guild);
        await scenario.Discord.InvokeSlashCommandAsync(moderator, "mod spam remove", guild);
        await scenario.Discord.InvokeSlashCommandAsync(moderator, "mod spam remove", guild);

        (await scenario.State.SpamChannelAsync(guild)).Should().BeFalse();
        scenario.DiscordApi.ShouldHaveConfigurationChanges(guild, moderator,
            ($"Spam channel <#{guild.ChannelId}>", "Disabled", "Enabled"),
            ($"Spam channel <#{guild.ChannelId}>", "Enabled", "Disabled"));
    }

    [Theory]
    [InlineData("server")]
    [InlineData("channel")]
    public async Task CommandAvailability_RepeatedChangesOnlyLogActualTransitions(string scope)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.ModerationLogAsync(guild);
        await scenario.Given.KnownCommandAsync("avatar");

        await scenario.Discord.InvokeSlashCommandAsync(moderator, $"command {scope}-disable", guild, arguments: [SlashArgument.Text("command", "avatar")]);
        await scenario.Discord.InvokeSlashCommandAsync(moderator, $"command {scope}-disable", guild, arguments: [SlashArgument.Text("command", "avatar")]);
        await scenario.Discord.InvokeSlashCommandAsync(moderator, $"command {scope}-enable", guild, arguments: [SlashArgument.Text("command", "avatar")]);
        await scenario.Discord.InvokeSlashCommandAsync(moderator, $"command {scope}-enable", guild, arguments: [SlashArgument.Text("command", "avatar")]);

        var setting = scope == "server" ? "Server command: avatar" : $"Channel command: avatar in <#{guild.ChannelId}>";
        scenario.DiscordApi.ShouldHaveConfigurationChanges(guild, moderator,
            (setting, "Enabled", "Disabled"), (setting, "Disabled", "Enabled"));
    }

    [Fact]
    public async Task Prefix_ChangeAndToggle_LogsValuesAndState()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.ModerationLogAsync(guild);

        await scenario.Discord.SendMessageAsync(moderator, guild, "!prefix .");
        await scenario.Discord.SendMessageAsync(moderator, guild, ".prefix .");
        var settings = await scenario.Discord.InvokeSlashCommandAsync(moderator, "command prefix", guild);
        await scenario.Discord.ClickAsync(moderator, settings, "Disable prefix commands");
        var updated = await scenario.Discord.InvokeSlashCommandAsync(moderator, "command prefix", guild);
        await scenario.Discord.ClickAsync(moderator, updated, "Enable prefix commands");

        (await scenario.State.PrefixAsync(guild)).Should().Be(".");
        scenario.DiscordApi.ShouldHaveConfigurationChanges(guild, moderator,
            ("Command prefix", "!", "."),
            ("Prefix commands", "Enabled", "Disabled"),
            ("Prefix commands", "Disabled", "Enabled"));
    }

    [Fact]
    public async Task ModLog_SameChannelOrCancelledMove_DoesNotLog()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        var nextChannel = await scenario.Given.TextChannelAsync(guild, "other");
        await scenario.Given.ModerationLogAsync(guild);

        var unchanged = await scenario.Discord.InvokeSlashCommandAsync(moderator, "mod log set", guild);
        await scenario.Discord.ClickAsync(moderator, unchanged, "Confirm");
        var move = await scenario.Discord.InvokeSlashCommandAsync(moderator, "mod log set", guild,
            arguments: [SlashArgument.Channel("channel", nextChannel)]);
        await scenario.Discord.ClickAsync(moderator, move, "Cancel");

        (await scenario.State.LogChannelAsync(guild, "mod")).Should().Be(guild.ChannelId);
        scenario.DiscordApi.ModerationLogs(guild).Should().BeEmpty();
    }

    [Fact]
    public async Task MissingModLog_ChangesConfigurationWithoutSendingAudit()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);

        var response = await scenario.Discord.InvokeSlashCommandAsync(moderator, "mod spam add", guild);

        response.ShouldBeSuccess();
        (await scenario.State.SpamChannelAsync(guild)).Should().BeTrue();
        scenario.DiscordApi.ModerationLogs(guild).Should().BeEmpty();
    }

    [Fact]
    public async Task DeletedModLog_LogsUnavailableChannelWithoutFailingChange()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.LogChannelAsync(guild, "mod", "100000000000000099");

        var response = await scenario.Discord.InvokeSlashCommandAsync(moderator, "mod spam add", guild);

        response.ShouldBeSuccess();
        (await scenario.State.SpamChannelAsync(guild)).Should().BeTrue();
        scenario.DiscordApi.ModerationLogs(guild).Should().BeEmpty();
        scenario.Logs.ToString().Should().Contain("Configuration audit channel 100000000000000099 is unavailable");
    }

    [Fact]
    public async Task DeliveryFailure_PreservesConfigurationAndCommandSuccess()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.ModerationLogAsync(guild);
        scenario.DiscordApi.ExpectRequest("POST", $"channels/{guild.ChannelId}/messages",
            new { code = 50013, message = "Missing Permissions" }, HttpStatusCode.Forbidden);

        var response = await scenario.Discord.InvokeSlashCommandAsync(moderator, "mod spam add", guild);

        response.ShouldBeSuccess();
        (await scenario.State.SpamChannelAsync(guild)).Should().BeTrue();
        scenario.DiscordApi.ModerationLogs(guild).Should().ContainSingle();
        scenario.Logs.ToString().Should().Contain("Could not send configuration audit");
    }

    [Fact]
    public async Task ModLog_OldChannelDeliveryFails_StillLogsToNewChannel()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        var nextChannel = await scenario.Given.TextChannelAsync(guild, "new-mod-log");
        await scenario.Given.ModerationLogAsync(guild);
        scenario.DiscordApi.ExpectModerationLog(nextChannel);
        scenario.DiscordApi.ExpectRequest("POST", $"channels/{guild.ChannelId}/messages",
            new { code = 50013, message = "Missing Permissions" }, HttpStatusCode.Forbidden);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(moderator, "mod log set", guild,
            arguments: [SlashArgument.Channel("channel", nextChannel)]);

        var response = await scenario.Discord.ClickAsync(moderator, prompt, "Confirm");

        response.ShouldBeSuccess();
        (await scenario.State.LogChannelAsync(guild, "mod")).Should().Be(nextChannel.ChannelId);
        scenario.DiscordApi.ShouldHaveConfigurationChanges(nextChannel, moderator,
            ("Moderation log channel", $"<#{guild.ChannelId}>", $"<#{nextChannel.ChannelId}>"));
        scenario.Logs.ToString().Should().Contain("Could not send configuration audit");
    }

    private static async Task EnableMonitoringAsync(CommandsDiscordScenario scenario, ScenarioUser moderator, ScenarioGuild guild, string kind)
    {
        var response = await scenario.Discord.InvokeSlashCommandAsync(moderator, $"monitor {kind} set", guild);
        if (kind != "members")
        {
            scenario.DiscordApi.ModerationLogs(guild).Should().BeEmpty();
            response = await scenario.Discord.ClickAsync(moderator, response, "Confirm");
        }
        response.ShouldBeSuccess();
    }
}
