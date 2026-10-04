using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Roles;

public sealed class AccessibleRoleTests(DataServices data)
{
    [Fact]
    public async Task Get_AlreadyOwnedRoleIsRejected()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.MemberAsync(guild, user, role);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!roles {role.Name}");

        response.ShouldBeError();
        response.Description.Should().Contain("already have");
        scenario.DiscordApi.RoleChanges.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_InaccessibleRoleIsRejected()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!roles {role.Name}");

        response.ShouldBeError();
        response.Description.Should().Contain("not marked as accessible");
        scenario.DiscordApi.RoleChanges.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_AccessibleRoleIsAssignedThroughDiscord()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.AccessibleRoleAsync(guild, role);
        scenario.DiscordApi.ExpectRoleChange(guild, user, role);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!roles {role.Name}");

        response.ShouldBeSuccess();
        response.Description.Should().Contain(role.Id);
        scenario.DiscordApi.RoleChanges.Should().ContainSingle().Which.Method.Should().Be("PUT");
    }

    [Fact]
    public async Task Get_ConflictingGroupRoleIsRejected()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var red = scenario.Given.Role("Red");
        var blue = scenario.Given.Role("Blue");
        var guild = await scenario.Given.GuildAsync(user, roles: [red, blue]);
        await scenario.Given.AccessibleRoleAsync(guild, red, group: "Colors");
        await scenario.Given.AccessibleRoleAsync(guild, blue, group: "Colors");
        await scenario.Given.MemberAsync(guild, user, red);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!roles Blue");

        response.ShouldBeError();
        response.Description.Should().Contain("Colors").And.Contain(red.Id);
        scenario.DiscordApi.RoleChanges.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_DiscordForbiddenExplainsRoleHierarchy()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.AccessibleRoleAsync(guild, role);
        scenario.DiscordApi.ExpectRoleChange(guild, user, role, forbidden: true);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!roles {role.Name}");

        response.ShouldBeError();
        response.Description.Should().Contain("Discord does not allow").And.Contain(role.Id);
        scenario.DiscordApi.RoleChanges.Should().ContainSingle();
    }

    [Fact]
    public async Task Drop_UnownedRoleIsRejected()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!roles drop {role.Name}");

        response.ShouldBeError();
        response.Description.Should().Contain("don't have");
        scenario.DiscordApi.RoleChanges.Should().BeEmpty();
    }

    [Fact]
    public async Task Drop_InaccessibleRoleCannotBeRemoved()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.MemberAsync(guild, user, role);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!roles drop {role.Name}");

        response.ShouldBeError();
        response.Description.Should().Contain("not accessible");
        scenario.DiscordApi.RoleChanges.Should().BeEmpty();
    }

    [Fact]
    public async Task Drop_AccessibleRoleIsRemovedThroughDiscord()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.AccessibleRoleAsync(guild, role);
        await scenario.Given.MemberAsync(guild, user, role);
        scenario.DiscordApi.ExpectRoleChange(guild, user, role, remove: true);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!roles drop {role.Name}");

        response.ShouldBeSuccess();
        response.Description.Should().Contain(role.Id);
        scenario.DiscordApi.RoleChanges.Should().ContainSingle().Which.Method.Should().Be("DELETE");
    }

    [Fact]
    public async Task Add_MakesRoleAccessible()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!roles add {role.Name}");

        response.ShouldBeSuccess();
        (await scenario.State.RoleAccessibleAsync(guild, role)).Should().BeTrue();
    }

    [Fact]
    public async Task Remove_MakesRoleInaccessible()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.AccessibleRoleAsync(guild, role);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!roles remove {role.Name}");

        response.ShouldBeSuccess();
        (await scenario.State.RoleAccessibleAsync(guild, role)).Should().BeFalse();
    }

    [Fact]
    public async Task Group_AssignsRoleToGroup()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!roles group Colors {role.Name}");

        response.ShouldBeSuccess();
        (await scenario.State.RoleGroupAsync(guild, role)).Should().Be("colors");
        (await scenario.State.RoleAccessibleAsync(guild, role)).Should().BeTrue();
    }

    [Fact]
    public async Task GroupClear_RemovesGroupWithoutChangingAccessibility()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.AccessibleRoleAsync(guild, role, group: "Colors");

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!roles group clear {role.Name}");

        response.ShouldBeSuccess();
        (await scenario.State.RoleGroupAsync(guild, role)).Should().BeNull();
        (await scenario.State.RoleAccessibleAsync(guild, role)).Should().BeTrue();
    }
}
