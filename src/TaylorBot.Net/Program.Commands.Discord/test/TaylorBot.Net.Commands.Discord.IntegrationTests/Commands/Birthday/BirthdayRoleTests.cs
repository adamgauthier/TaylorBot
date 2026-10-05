using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Birthday;

[Trait("Command", "birthday role")]
public sealed class BirthdayRoleTests(DataServices data)
{
    [Fact]
    public async Task Role_RequiresManageRolesPermission()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync();
        var user = await scenario.Given.UserAsync(username: "Member");
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.MemberAsync(guild, user);
        await scenario.Given.PlusAsync(owner, maximumGuilds: 2, guild);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday role", guild, permissions: "0");

        response.ShouldBeError();
        response.Description.Should().Contain("Manage Roles");
        (await scenario.State.BirthdayRoleAsync(guild)).Should().BeNull();
    }

    [Fact]
    public async Task Role_NonPlusServerCannotConfigureBirthdayRole()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday role", guild);

        response.ShouldBeError();
        response.Description.Should().Contain("Plus");
        (await scenario.State.BirthdayRoleAsync(guild)).Should().BeNull();
    }

    [Fact]
    public async Task Role_CreatePersistsReturnedDiscordRole()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.PlusAsync(user, maximumGuilds: 2, guild);
        await scenario.Given.ModerationLogAsync(guild);
        var role = scenario.Given.Role("happy birthday");
        scenario.Given.BirthdayRoleCreation(guild, role);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday role", guild);
        prompt.Description.Should().Contain("no birthday role");
        scenario.DiscordApi.ModerationLogs(guild).Should().BeEmpty();

        var response = await scenario.Discord.ClickAsync(user, prompt, "Create birthday role");

        response.ShouldBeSuccess();
        response.Description.Should().Contain(role.Id);
        (await scenario.State.BirthdayRoleAsync(guild)).Should().Be(role.Id);
        scenario.DiscordApi.ShouldHaveConfigurationChanges(guild, user, ("Birthday role", "Not configured", $"<@&{role.Id}>"));
        scenario.DiscordApi.RequestsFor("POST", $"guilds/{guild.Id}/roles").Should().ContainSingle()
            .Which.Body!.Value.GetProperty("name").GetString().Should().Be("happy birthday");
    }

    [Fact]
    public async Task Role_RemoveDeletesDiscordRoleAndConfiguration()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role("happy birthday");
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.PlusAsync(user, maximumGuilds: 2, guild);
        await scenario.Given.ModerationLogAsync(guild);
        await scenario.Given.BirthdayRoleAsync(guild, role);
        await scenario.Given.BirthdayRoleAwardAsync(guild, user, role);
        scenario.Given.BirthdayRoleDeletion(guild, role);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday role", guild);
        prompt.Description.Should().Contain(role.Id);

        var response = await scenario.Discord.ClickAsync(user, prompt, "Remove birthday role");

        response.ShouldBeSuccess();
        (await scenario.State.BirthdayRoleAsync(guild)).Should().BeNull();
        (await scenario.State.BirthdayRoleAwardsAsync(guild)).Should().Be(0);
        scenario.DiscordApi.ShouldHaveConfigurationChanges(guild, user, ("Birthday role", $"<@&{role.Id}>", "Not configured"));
    }

    [Fact]
    public async Task Role_DeletedRoleCanBeRecreated()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.PlusAsync(user, maximumGuilds: 2, guild);
        var previous = scenario.Given.Role("deleted");
        await scenario.Given.BirthdayRoleAsync(guild, previous);
        await scenario.Given.ModerationLogAsync(guild);
        var replacement = scenario.Given.Role("happy birthday");
        scenario.Given.BirthdayRoleCreation(guild, replacement);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday role", guild);
        prompt.Description.Should().Contain("deleted");

        var response = await scenario.Discord.ClickAsync(user, prompt, "Re-create birthday role");

        response.ShouldBeSuccess();
        (await scenario.State.BirthdayRoleAsync(guild)).Should().Be(replacement.Id);
        scenario.DiscordApi.ShouldHaveConfigurationChanges(guild, user, ("Birthday role", $"<@&{previous.Id}>", $"<@&{replacement.Id}>"));
    }

    [Fact]
    public async Task Role_DeletedRoleConfigurationCanBeRemoved()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.PlusAsync(user, maximumGuilds: 2, guild);
        var previous = scenario.Given.Role("deleted");
        await scenario.Given.BirthdayRoleAsync(guild, previous);
        await scenario.Given.ModerationLogAsync(guild);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday role", guild);

        var response = await scenario.Discord.ClickAsync(user, prompt, "Remove birthday role");

        response.ShouldBeSuccess();
        (await scenario.State.BirthdayRoleAsync(guild)).Should().BeNull();
        scenario.DiscordApi.ShouldHaveConfigurationChanges(guild, user, ("Birthday role", $"<@&{previous.Id}>", "Not configured"));
    }

    [Fact]
    public async Task Role_MemberWithoutPermissionCannotRemoveConfiguration()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync();
        var role = scenario.Given.Role("happy birthday");
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.MemberAsync(guild, other);
        await scenario.Given.PlusAsync(user, maximumGuilds: 2, guild);
        await scenario.Given.BirthdayRoleAsync(guild, role);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday role", guild);

        var response = await scenario.Discord.ClickAsync(other, prompt, "Remove birthday role");

        response.ShouldBeError();
        response.Description.Should().Contain("Manage Roles");
        (await scenario.State.BirthdayRoleAsync(guild)).Should().Be(role.Id);
    }

    [Fact]
    public async Task Role_AnotherPrivilegedUserCannotRemoveConfiguration()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync(botOwner: true);
        var role = scenario.Given.Role("happy birthday");
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.MemberAsync(guild, other);
        await scenario.Given.PlusAsync(user, maximumGuilds: 2, guild);
        await scenario.Given.BirthdayRoleAsync(guild, role);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday role", guild);

        var response = await scenario.Discord.ClickAsync(other, prompt, "Remove birthday role");

        response.Requests.Should().ContainSingle("another user's click is acknowledged without editing the original message");
        (await scenario.State.BirthdayRoleAsync(guild)).Should().Be(role.Id);
    }
}
