using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Jail;

public sealed class JailTests(DataServices data)
{
    [Fact]
    public async Task Jail_WithoutConfiguredRoleExplainsSetup()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var target = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.MemberAsync(guild, target);

        var response = await scenario.Discord.SendMessageAsync(moderator, guild, $"!jail <@{target.Id}>");

        response.ShouldBeError();
        response.Description.Should().Contain("No jail role has been set");
        scenario.DiscordApi.RoleChanges.Should().BeEmpty();
    }

    [Fact]
    public async Task Jail_DeletedRoleExplainsReconfiguration()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var target = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.MemberAsync(guild, target);
        await scenario.Given.JailRoleAsync(guild, scenario.Given.Role("Deleted"));

        var response = await scenario.Discord.SendMessageAsync(moderator, guild, $"!jail <@{target.Id}>");

        response.ShouldBeError();
        response.Description.Should().Contain("could not be found");
        scenario.DiscordApi.RoleChanges.Should().BeEmpty();
    }

    [Fact]
    public async Task Jail_DiscordForbiddenExplainsMissingPermission()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var target = await scenario.Given.UserAsync();
        var role = scenario.Given.Role("Jailed");
        var guild = await scenario.Given.GuildAsync(moderator, roles: [role]);
        await scenario.Given.MemberAsync(guild, target);
        await scenario.Given.JailRoleAsync(guild, role);
        scenario.DiscordApi.ExpectRoleChange(guild, target, role, forbidden: true);

        var response = await scenario.Discord.SendMessageAsync(moderator, guild, $"!jail <@{target.Id}>");

        response.ShouldBeError();
        response.Description.Should().Contain("missing permissions");
        scenario.DiscordApi.RoleChanges.Should().ContainSingle();
    }

    [Fact]
    public async Task Jail_AssignsConfiguredRole()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var target = await scenario.Given.UserAsync();
        var role = scenario.Given.Role("Jailed");
        var guild = await scenario.Given.GuildAsync(moderator, roles: [role]);
        await scenario.Given.MemberAsync(guild, target);
        await scenario.Given.JailRoleAsync(guild, role);
        scenario.DiscordApi.ExpectRoleChange(guild, target, role);
        await scenario.Given.ModerationLogAsync(guild);

        var response = await scenario.Discord.SendMessageAsync(moderator, guild, $"!jail <@{target.Id}>");

        response.ShouldBeSuccess();
        response.Description.Should().Contain(target.Id).And.Contain("successfully jailed");
        scenario.DiscordApi.RoleChanges.Should().ContainSingle().Which.Method.Should().Be("PUT");
        scenario.DiscordApi.ModerationLogs(guild).Should().ContainSingle().Which.Body!.Value.GetRawText().Should().Contain(target.Id).And.Contain(moderator.Id);
    }

    [Fact]
    public async Task Free_RemovesConfiguredRole()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var target = await scenario.Given.UserAsync();
        var role = scenario.Given.Role("Jailed");
        var guild = await scenario.Given.GuildAsync(moderator, roles: [role]);
        await scenario.Given.MemberAsync(guild, target, role);
        await scenario.Given.JailRoleAsync(guild, role);
        scenario.DiscordApi.ExpectRoleChange(guild, target, role, remove: true);
        await scenario.Given.ModerationLogAsync(guild);

        var response = await scenario.Discord.SendMessageAsync(moderator, guild, $"!jail free <@{target.Id}>");

        response.ShouldBeSuccess();
        response.Description.Should().Contain(target.Id).And.Contain("successfully freed");
        scenario.DiscordApi.RoleChanges.Should().ContainSingle().Which.Method.Should().Be("DELETE");
        scenario.DiscordApi.ModerationLogs(guild).Should().ContainSingle().Which.Body!.Value.GetRawText().Should().Contain(target.Id).And.Contain(moderator.Id);
    }

    [Fact]
    public async Task Set_PersistsJailRole()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var role = scenario.Given.Role("Jailed");
        var guild = await scenario.Given.GuildAsync(moderator, roles: [role]);

        var response = await scenario.Discord.SendMessageAsync(moderator, guild, $"!jail set {role.Name}");

        response.ShouldBeSuccess();
        (await scenario.State.JailRoleAsync(guild)).Should().Be(role.Id);
    }
}
