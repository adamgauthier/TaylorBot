using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Events;

[Trait("Command", "love spread")]
[Trait("Command", "love history")]
[Trait("Command", "love ready")]
[Trait("Command", "love leaderboard")]
public sealed class LoveTests(DataServices data)
{
    private const string MissingSchema = "Blocked: valentines2026 is manually provisioned outside Sqitch; do not create a test-only schema.";

    [Fact(Skip = MissingSchema)]
    public async Task Spread_AssignsRoleRecordsSenderAndWelcomesRecipient()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var sender = await scenario.Given.UserAsync(username: "Alice");
        var recipient = await scenario.Given.UserAsync(username: "Bob");
        var role = scenario.Given.Role("Lover");
        var guild = await scenario.Given.EventGuildAsync(sender, role);
        await scenario.Given.MemberAsync(guild, sender, role);
        await scenario.Given.MemberAsync(guild, recipient);
        await scenario.Given.LoveEventAsync(guild, role);
        await scenario.Given.LoveReceivedAsync(sender, sender);
        scenario.DiscordApi.ExpectRoleChange(guild, recipient, role);
        scenario.DiscordApi.ExpectModerationLog(guild);

        var response = await scenario.Discord.InvokeSlashCommandAsync(sender, "love spread", guild, selectedUser: recipient);

        response.ShouldBeSuccess();
        response.Description.Should().Contain(recipient.Id).And.Contain("delivered");
        (await scenario.State.LoveSenderAsync(recipient)).Should().Be(sender.Id);
        scenario.DiscordApi.RoleChanges.Should().ContainSingle().Which.Method.Should().Be("PUT");
        scenario.DiscordApi.ModerationLogs(guild).Should().ContainSingle().Which.Body!.Value
            .GetProperty("content").GetString().Should().Contain(recipient.Id).And.Contain(sender.Id);
    }

    [Fact(Skip = MissingSchema)]
    public async Task HistoryLeaderboardAndReady_ShowPersistedLoveChain()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var sender = await scenario.Given.UserAsync(username: "Alice");
        var recipient = await scenario.Given.UserAsync(username: "Bob");
        var role = scenario.Given.Role("Lover");
        var guild = await scenario.Given.EventGuildAsync(sender, role);
        await scenario.Given.MemberAsync(guild, recipient);
        await scenario.Given.LoveEventAsync(guild, role);
        await scenario.Given.LoveReceivedAsync(sender, sender);
        await scenario.Given.LoveReceivedAsync(recipient, sender);

        var history = await scenario.Discord.InvokeSlashCommandAsync(sender, "love history", guild, selectedUser: recipient);
        var leaderboard = await scenario.Discord.InvokeSlashCommandAsync(sender, "love leaderboard", guild);
        var ready = await scenario.Discord.InvokeSlashCommandAsync(sender, "love ready", guild);

        history.Description.Should().Contain(sender.Id).And.Contain(recipient.Id);
        leaderboard.Description.Should().Contain(recipient.Id).And.Contain("1");
        ready.Description.Should().Contain(sender.Id).And.Contain(recipient.Id);
    }

    [Fact(Skip = MissingSchema)]
    public async Task Spread_ClosedEventDoesNotAssignRole()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var sender = await scenario.Given.UserAsync();
        var recipient = await scenario.Given.UserAsync();
        var role = scenario.Given.Role("Lover");
        var guild = await scenario.Given.EventGuildAsync(sender, role);
        await scenario.Given.MemberAsync(guild, recipient);
        await scenario.Given.LoveEventAsync(guild, role, ended: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(sender, "love spread", guild, selectedUser: recipient);

        response.ShouldBeError();
        response.Description.Should().Contain("has ended");
        scenario.DiscordApi.RoleChanges.Should().BeEmpty();
        (await scenario.State.LoveSenderAsync(recipient)).Should().BeNull();
    }

    [Fact(Skip = MissingSchema)]
    public async Task Spread_IncubatingSenderMustWait()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var sender = await scenario.Given.UserAsync();
        var recipient = await scenario.Given.UserAsync();
        var role = scenario.Given.Role("Lover");
        var guild = await scenario.Given.EventGuildAsync(sender, role);
        await scenario.Given.MemberAsync(guild, sender, role);
        await scenario.Given.MemberAsync(guild, recipient);
        await scenario.Given.LoveEventAsync(guild, role);
        await scenario.Given.LoveReceivedAsync(sender, sender, hoursAgo: 0);

        var response = await scenario.Discord.InvokeSlashCommandAsync(sender, "love spread", guild, selectedUser: recipient);

        response.ShouldBeError();
        response.Description.Should().Contain("wait a little more");
        scenario.DiscordApi.RoleChanges.Should().BeEmpty();
        (await scenario.State.LoveSenderAsync(recipient)).Should().BeNull();
    }
}
