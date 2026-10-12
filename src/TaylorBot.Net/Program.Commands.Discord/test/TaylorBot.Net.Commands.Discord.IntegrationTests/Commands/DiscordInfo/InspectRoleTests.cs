using System.Net;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;
using TaylorBot.Net.Commands.PostExecution;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.DiscordInfo;

[Trait("Command", "inspect role")]
public sealed class InspectRoleTests(DataServices data)
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(13, true)]
    public async Task Inspect_ShowsExactCountAndMarksIncompleteCachedMentions(int count, bool incomplete)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.MemberAsync(guild, user, role);
        scenario.DiscordApi.ExpectRoleMemberCount(guild, role, count);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect role", guild,
            arguments: [SlashArgument.Role("role", role)]);

        response.Field("Members").Should().Contain($"({count})").And.Contain($"<@{user.Id}>").And.NotContain("+").And.NotContain(user.Username);
        response.Field("Members").EndsWith("...", StringComparison.Ordinal).Should().Be(incomplete);
        scenario.DiscordApi.RequestsFor("GET", $"guilds/{guild.Id}/members").Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(13, true)]
    public async Task Inspect_ShowsExactCountWithoutCachedMentions(int count, bool incomplete)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        scenario.DiscordApi.ExpectRoleMemberCount(guild, role, count);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect role", guild,
            arguments: [SlashArgument.Role("role", role)]);

        response.Field("Members").Should().Contain($"({count})").And.NotContain("+").And.NotContain($"<@{user.Id}>");
        response.Field("Members").EndsWith("...", StringComparison.Ordinal).Should().Be(incomplete);
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(13, true)]
    public async Task Inspect_ListsSpaceSeparatedMentionsAndMarksMissingMembers(int count, bool incomplete)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var members = await scenario.Given.UsersAsync(count: 4);
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.MembersAsync(guild, members, role);
        scenario.DiscordApi.ExpectRoleMemberCount(guild, role, count);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect role", guild,
            arguments: [SlashArgument.Role("role", role)]);

        var preview = response.Field("Members").Split("** ", StringSplitOptions.None)[1];
        preview.Split(' ').Where(token => token != "...").Should().BeEquivalentTo(members.Select(member => $"<@{member.Id}>"));
        preview.Length.Should().BeLessThanOrEqualTo(100);
        preview.EndsWith("...", StringComparison.Ordinal).Should().Be(incomplete);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(13)]
    public async Task Inspect_TruncatesCachedMentionsWithoutSplittingThem(int count)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var members = await scenario.Given.UsersAsync(count: 5);
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.MembersAsync(guild, members, role);
        scenario.DiscordApi.ExpectRoleMemberCount(guild, role, count);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect role", guild,
            arguments: [SlashArgument.Role("role", role)]);

        var preview = response.Field("Members").Split("** ", StringSplitOptions.None)[1];
        var tokens = preview.Split(' ');
        var expected = members.Select(member => $"<@{member.Id}>").ToList();
        tokens.Last().Should().Be("...");
        tokens[..^1].Should().OnlyContain(mention => expected.Contains(mention)).And.OnlyHaveUniqueItems();
        tokens.Length.Should().Be(5);
        preview.Length.Should().BeLessThanOrEqualTo(100);
        response.Field("Members").Should().Contain($"({count})").And.NotContain("+");
    }

    [Fact]
    public async Task Inspect_EveryoneRetainsCachedCountWithoutCountRequest()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        ScenarioRole role = new(guild.Id, "@everyone");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect role", guild,
            arguments: [SlashArgument.Role("role", role)]);

        response.Field("Members").Should().Contain("(2+)").And.Contain($"<@{user.Id}>").And.Contain($"<@{DiscordApi.ApplicationId}>");
        scenario.DiscordApi.RequestsFor("GET", $"guilds/{guild.Id}/roles/member-counts").Should().BeEmpty();
    }

    [Fact]
    public async Task Inspect_CountFailureDoesNotSilentlyUseCache()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.MemberAsync(guild, user, role);
        scenario.DiscordApi.ExpectRequest("GET", $"guilds/{guild.Id}/roles/member-counts",
            new { code = 50001, message = "Missing Access" }, HttpStatusCode.Forbidden);
        scenario.Logs.ExpectError<SlashCommandHandler>("Unhandled exception in slash command");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect role", guild,
            arguments: [SlashArgument.Role("role", role)]);

        response.ShouldBeError();
    }

    [Fact]
    public async Task Inspect_MissingRoleCountDoesNotSilentlyUseCache()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);
        await scenario.Given.MemberAsync(guild, user, role);
        scenario.DiscordApi.ExpectRequest("GET", $"guilds/{guild.Id}/roles/member-counts",
            new Dictionary<string, int>(), HttpStatusCode.OK);
        scenario.Logs.ExpectError<SlashCommandHandler>("Unhandled exception in slash command");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect role", guild,
            arguments: [SlashArgument.Role("role", role)]);

        response.ShouldBeError();
    }
}
