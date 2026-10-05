using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Moderation;

[Trait("Command", "command server-disable")]
[Trait("Command", "command server-enable")]
[Trait("Command", "command channel-disable")]
[Trait("Command", "command channel-enable")]
[Trait("Command", "command prefix")]
public sealed class CommandSettingsTests(DataServices data)
{
    [Theory]
    [InlineData("command server-disable")]
    [InlineData("command server-enable")]
    [InlineData("command channel-disable")]
    [InlineData("command channel-enable")]
    public async Task CommandSettings_RequireManagementPermissions(string route)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync();
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.MemberAsync(guild, user);
        await scenario.Given.KnownCommandAsync("avatar");
        await scenario.Given.ModerationLogAsync(guild);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, route, guild,
            arguments: [SlashArgument.Text("command", "avatar")], permissions: "0");

        response.ShouldBeError();
        response.Description.Should().Contain("permission");
        (await scenario.State.ServerCommandDisabledAsync(guild, "avatar")).Should().BeFalse();
        (await scenario.State.ChannelCommandDisabledAsync(guild, "avatar")).Should().BeFalse();
        scenario.DiscordApi.ModerationLogs(guild).Should().BeEmpty();
    }

    [Fact]
    public async Task ServerDisableAndEnable_PersistAndRestoreCommandAvailability()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.KnownCommandAsync("avatar");

        var disabled = await scenario.Discord.InvokeSlashCommandAsync(user, "command server-disable", guild, arguments: [SlashArgument.Text("command", " AVATAR ")]);
        var blocked = await scenario.Discord.InvokeSlashCommandAsync(user, "avatar", guild);

        disabled.ShouldBeSuccess();
        blocked.Description.Should().Contain("disabled");
        (await scenario.State.ServerCommandDisabledAsync(guild, "avatar")).Should().BeTrue();

        var enabled = await scenario.Discord.InvokeSlashCommandAsync(user, "command server-enable", guild, arguments: [SlashArgument.Text("command", "avatar")]);
        var available = await scenario.Discord.InvokeSlashCommandAsync(user, "avatar", guild);

        enabled.ShouldBeSuccess();
        available.Embed.GetProperty("image").GetProperty("url").GetString().Should().Contain("discord");
        (await scenario.State.ServerCommandDisabledAsync(guild, "avatar")).Should().BeFalse();
    }

    [Fact]
    public async Task ServerEnable_LeavesOtherCommandsDisabled()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.KnownCommandAsync("avatar");
        await scenario.Given.KnownCommandAsync("choose");
        await scenario.Given.DisabledServerCommandAsync(guild, "avatar");
        await scenario.Given.DisabledServerCommandAsync(guild, "choose");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "command server-enable", guild, arguments: [SlashArgument.Text("command", "avatar")]);

        response.ShouldBeSuccess();
        (await scenario.State.ServerCommandDisabledAsync(guild, "avatar")).Should().BeFalse();
        (await scenario.State.ServerCommandDisabledAsync(guild, "choose")).Should().BeTrue();
    }

    [Fact]
    public async Task ChannelDisableAndEnable_PersistSelectedChannel()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.KnownCommandAsync("avatar");

        var disabled = await scenario.Discord.InvokeSlashCommandAsync(user, "command channel-disable", guild,
            arguments: [SlashArgument.Text("command", "avatar"), SlashArgument.Channel("channel", guild)]);

        disabled.ShouldBeSuccess();
        disabled.Description.Should().Contain(guild.ChannelId);
        (await scenario.State.ChannelCommandDisabledAsync(guild, "avatar")).Should().BeTrue();

        var enabled = await scenario.Discord.InvokeSlashCommandAsync(user, "command channel-enable", guild, arguments: [SlashArgument.Text("command", "avatar")]);

        enabled.ShouldBeSuccess();
        (await scenario.State.ChannelCommandDisabledAsync(guild, "avatar")).Should().BeFalse();
    }

    [Theory]
    [InlineData("command server-disable")]
    [InlineData("command server-enable")]
    [InlineData("command channel-disable")]
    [InlineData("command channel-enable")]
    public async Task UnknownCommand_IsRejected(string route)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, route, guild, arguments: [SlashArgument.Text("command", "not-a-command")]);

        response.ShouldBeError();
        response.Description.Should().Contain("Could not find command");
    }

    [Theory]
    [InlineData("command server-disable", "command prefix", "essential")]
    [InlineData("command channel-disable", "owner reward", "essential")]
    [InlineData("command server-disable", "modmail message-mods", "Integrations")]
    [InlineData("command channel-disable", "modmail message-mods", "Integrations")]
    public async Task ProtectedCommand_CannotBeDisabled(string route, string command, string explanation)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.KnownCommandAsync(command);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, route, guild, arguments: [SlashArgument.Text("command", command)]);

        response.ShouldBeError();
        response.Description.Should().Contain(explanation);
        (await scenario.State.ServerCommandDisabledAsync(guild, command)).Should().BeFalse();
        (await scenario.State.ChannelCommandDisabledAsync(guild, command)).Should().BeFalse();
    }

    [Fact]
    public async Task Prefix_ButtonsDisableAndReenableLegacyCommands()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var settings = await scenario.Discord.InvokeSlashCommandAsync(user, "command prefix", guild);
        var disabled = await scenario.Discord.ClickAsync(user, settings, "Disable prefix commands");

        settings.Description.Should().Contain("`!`");
        disabled.ShouldBeSuccess();
        (await scenario.State.ServerCommandDisabledAsync(guild, "all-prefix")).Should().BeTrue();

        var updated = await scenario.Discord.InvokeSlashCommandAsync(user, "command prefix", guild);
        var enabled = await scenario.Discord.ClickAsync(user, updated, "Enable prefix commands");
        var legacy = await scenario.Discord.SendMessageAsync(user, guild, "!prefix");

        enabled.ShouldBeSuccess();
        (await scenario.State.ServerCommandDisabledAsync(guild, "all-prefix")).Should().BeFalse();
        legacy.Description.Should().Contain("The command prefix for this server is `!`");
    }

    [Fact]
    public async Task Prefix_AnotherModeratorCannotUseOriginalUsersToggle()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.MemberAsync(guild, other);

        var settings = await scenario.Discord.InvokeSlashCommandAsync(owner, "command prefix", guild);
        var response = await scenario.Discord.ClickAsync(other, settings, "Disable prefix commands");

        response.ShouldBeError();
        (await scenario.State.ServerCommandDisabledAsync(guild, "all-prefix")).Should().BeFalse();
    }
}
