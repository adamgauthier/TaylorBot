using FluentAssertions;
using System.Net;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Moderation;

[Trait("Command", "kick")]
public sealed class KickTests(DataServices data)
{
    [Fact]
    public async Task LongstandingMember_RequiresConfirmationBeforeKickAndLogsReason()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var member = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.MemberAsync(guild, member);
        await scenario.Given.ModerationLogAsync(guild);
        scenario.DiscordApi.ExpectRequest("DELETE", $"guilds/{guild.Id}/members/{member.Id}");

        var prompt = await scenario.Discord.InvokeSlashCommandAsync(moderator, "kick", guild,
            arguments: [SlashArgument.User("member", member), SlashArgument.Text("reason", "Repeated rule violations")]);

        prompt.Description.Should().Contain("Are you sure");
        scenario.DiscordApi.RequestsFor("DELETE", $"guilds/{guild.Id}/members/{member.Id}").Should().BeEmpty();

        var response = await scenario.Discord.ClickAsync(moderator, prompt, "Confirm");

        response.Description.Should().Contain("successfully kicked");
        scenario.DiscordApi.RequestsFor("DELETE", $"guilds/{guild.Id}/members/{member.Id}").Should().ContainSingle();
        scenario.DiscordApi.ModerationLogs(guild).Should().ContainSingle().Which.Body!.Value.GetProperty("embeds")[0]
            .GetProperty("fields").EnumerateArray().Should().Contain(field => field.GetProperty("name").GetString() == "Reason"
                && field.GetProperty("value").GetString() == "Repeated rule violations");
    }

    [Fact]
    public async Task MissingDiscordPermission_ExplainsRoleHierarchy()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var member = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.MemberAsync(guild, member);
        scenario.DiscordApi.ExpectRequest("DELETE", $"guilds/{guild.Id}/members/{member.Id}",
            new { code = 50013, message = "Missing Permissions" }, HttpStatusCode.Forbidden);

        var prompt = await scenario.Discord.InvokeSlashCommandAsync(moderator, "kick", guild,
            arguments: [SlashArgument.User("member", member), SlashArgument.Text("reason", "Repeated rule violations")]);
        var response = await scenario.Discord.ClickAsync(moderator, prompt, "Confirm");

        response.ShouldBeError();
        response.Description.Should().Contain("missing permissions").And.Contain("higher in the list");
    }

    [Fact]
    public async Task ConfirmWithoutOptionalReason_KicksMember()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var member = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.MemberAsync(guild, member);
        await scenario.Given.ModerationLogAsync(guild);

        var prompt = await scenario.Discord.InvokeSlashCommandAsync(moderator, "kick", guild,
            arguments: [SlashArgument.User("member", member)]);
        scenario.DiscordApi.ExpectRequest("DELETE", $"guilds/{guild.Id}/members/{member.Id}");
        var response = await scenario.Discord.ClickAsync(moderator, prompt, "Confirm");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("successfully kicked");
        scenario.DiscordApi.RequestsFor("DELETE", $"guilds/{guild.Id}/members/{member.Id}").Should().ContainSingle();
    }

    [Fact]
    public async Task Cancel_DoesNotKickMember()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var member = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.MemberAsync(guild, member);

        var prompt = await scenario.Discord.InvokeSlashCommandAsync(moderator, "kick", guild, arguments: [SlashArgument.User("member", member)]);
        var response = await scenario.Discord.ClickAsync(moderator, prompt, "Cancel");

        response.Description.Should().ContainEquivalentOf("cancel");
        scenario.DiscordApi.RequestsFor("DELETE", $"guilds/{guild.Id}/members/{member.Id}").Should().BeEmpty();
    }

    [Fact]
    public async Task ServerOwner_CannotBeKicked()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync();
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.MemberAsync(guild, moderator);

        var response = await scenario.Discord.InvokeSlashCommandAsync(moderator, "kick", guild,
            arguments: [SlashArgument.User("member", owner)], permissions: "2");

        response.ShouldBeError();
        response.Description.Should().Contain("server owner");
        scenario.DiscordApi.RequestsFor("DELETE", $"guilds/{guild.Id}/members/{owner.Id}").Should().BeEmpty();
    }

    [Fact]
    public async Task SelfKick_IsRejectedBeforeDiscordMutation()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);

        var response = await scenario.Discord.InvokeSlashCommandAsync(moderator, "kick", guild, arguments: [SlashArgument.User("member", moderator)]);

        response.ShouldBeError();
        response.Description.Should().Contain("yourself");
        scenario.DiscordApi.RequestsFor("DELETE", $"guilds/{guild.Id}/members/{moderator.Id}").Should().BeEmpty();
    }

    [Fact]
    public async Task EqualHighestRoles_PreventModeratorFromKickingPeer()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync();
        var moderator = await scenario.Given.UserAsync();
        var member = await scenario.Given.UserAsync();
        var role = scenario.Given.Role("Moderators");
        var guild = await scenario.Given.GuildAsync(owner, roles: [role]);
        await scenario.Given.MemberAsync(guild, moderator, role);
        await scenario.Given.MemberAsync(guild, member, role);

        var response = await scenario.Discord.InvokeSlashCommandAsync(moderator, "kick", guild,
            arguments: [SlashArgument.User("member", member)], permissions: "2");

        response.ShouldBeError();
        response.Description.Should().Contain("equal to or higher");
        scenario.DiscordApi.RequestsFor("DELETE", $"guilds/{guild.Id}/members/{member.Id}").Should().BeEmpty();
    }
}
