using FluentAssertions;
using System.Net;
using System.Globalization;
using System.Text.RegularExpressions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;
using TaylorBot.Net.Commands.PostExecution;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Moderation;

[Trait("Command", "owner reward-users")]
public sealed class OwnerRewardUsersTests(DataServices data)
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Reward_RejectsNonPositiveAmountBeforeResolvingUsers(long amount)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var recipient = await scenario.Given.UserAsync(taypoints: 9);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", arguments:
            [SlashArgument.Integer("amount", amount), SlashArgument.Text("users", recipient.Id)]);

        response.ShouldBeError();
        (await scenario.State.BalanceAsync(recipient)).Should().Be(9);
        scenario.DiscordApi.RequestsFor("GET", $"users/{recipient.Id}").Should().BeEmpty();
    }

    [Fact]
    public async Task Reward_RejectsNonOwnerBeforeResolvingUsers()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var recipient = await scenario.Given.UserAsync(taypoints: 9);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "owner reward-users", arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.Text("users", recipient.Id)]);

        response.ShouldBeError();
        response.Description.Should().Contain("owner");
        (await scenario.State.BalanceAsync(recipient)).Should().Be(9);
        scenario.DiscordApi.RequestsFor("GET", $"users/{recipient.Id}").Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid 0 18446744073709551616 <@&100000000000000005>")]
    public async Task Reward_RejectsEmptyOrInvalidInput(string input)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.Text("users", input)]);

        response.ShouldBeError();
        response.Description.Should().NotBeNullOrWhiteSpace();
        (await scenario.State.BalanceAsync(owner)).Should().Be(0);
    }

    [Fact]
    public async Task Reward_RejectsRoleMentionWithoutRewardingCachedMembers()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(taypoints: 9, botOwner: true);
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(owner, roles: [role]);
        await scenario.Given.MemberAsync(guild, owner, role);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", guild, arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.Text("users", $"<@&{role.Id}>")]);

        response.ShouldBeError();
        response.Description.Should().Contain("invalid user mention or user ID");
        (await scenario.State.BalanceAsync(owner)).Should().Be(9);
        scenario.DiscordApi.RequestsFor("GET", $"guilds/{guild.Id}").Should().BeEmpty();
        scenario.DiscordApi.RequestsFor("GET", $"guilds/{guild.Id}/members").Should().BeEmpty();
    }

    [Theory]
    [InlineData("<@&100000000000000999>")]
    [InlineData("@everyone")]
    [InlineData("@here")]
    public async Task Reward_RejectsGroupMentionsAndRewardsOnlyExplicitUsers(string groupMention)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(taypoints: 9, botOwner: true);
        var recipient = await scenario.Given.UserAsync(taypoints: 31);
        ScenarioRole role = new("100000000000000999", "Recipients");
        var guild = await scenario.Given.GuildAsync(owner, roles: [role]);
        await scenario.Given.MemberAsync(guild, owner, role);
        await scenario.Given.MemberAsync(guild, recipient, role);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", guild, arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.Text("users", $"{groupMention} <@{recipient.Id}>")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Rejected 1 recipient").And.Contain("invalid user mention or user ID");
        (await scenario.State.BalanceAsync(owner)).Should().Be(9);
        (await scenario.State.BalanceAsync(recipient)).Should().Be(44);
        scenario.DiscordApi.RequestsFor("GET", $"guilds/{guild.Id}").Should().BeEmpty();
        scenario.DiscordApi.RequestsFor("GET", $"guilds/{guild.Id}/members").Should().BeEmpty();
    }

    [Fact]
    public async Task Reward_ReportsUnknownUserAndRewardsValidRecipient()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var recipient = await scenario.Given.UserAsync(taypoints: 9);
        const string unknown = "100000000000009999";
        scenario.DiscordApi.ExpectRequest("GET", $"users/{unknown}",
            new { code = 10013, message = "Unknown User" }, HttpStatusCode.NotFound);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.Text("users", $"{unknown} {recipient.Id} <@{unknown}>")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain(unknown).And.Contain("not found").And.Contain(recipient.Id);
        response.Description.Should().Contain("Rejected 1 recipient");
        (await scenario.State.BalanceAsync(recipient)).Should().Be(22);
        scenario.DiscordApi.RequestsFor("GET", $"users/{unknown}").Should().ContainSingle();
    }

    [Fact]
    public async Task Reward_ReportsUnknownUserWithoutClaimingSuccess()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        const string unknown = "100000000000009999";
        scenario.DiscordApi.ExpectRequest("GET", $"users/{unknown}",
            new { code = 10013, message = "Unknown User" }, HttpStatusCode.NotFound);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.Text("users", unknown)]);

        response.ShouldBeError();
        response.Description.Should().Contain(unknown).And.Contain("not found");
        (await scenario.State.BalanceAsync(owner)).Should().Be(0);
    }

    [Fact]
    public async Task Reward_TracksPreviouslyUnknownRecipient()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var recipient = scenario.Given.UntrackedUser();

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.Text("users", recipient.Id)]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain(recipient.Id).And.Contain("13");
        (await scenario.State.BalanceAsync(recipient)).Should().Be(13);
    }

    [Fact]
    public async Task Reward_PaginatesAllResultsWithoutRepeatingRewards()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var recipients = await scenario.Given.UsersAsync(count: 150, taypoints: 9);

        var first = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.Text("users", string.Join(' ', recipients.Select(user => user.Id)))]);
        var next = await scenario.Discord.ClickAsync(owner, first, "Next");
        var previous = await scenario.Discord.ClickAsync(owner, next, "Previous");
        var pages = await ReadPagesAsync(scenario, owner, first);

        pages.Should().OnlyContain(page => page.Description.Length <= 4096);
        Regex.Matches(string.Join('\n', pages.Select(page => page.Description)), @"(?m)^<@(\d+)>:").Select(match => match.Groups[1].Value)
            .Should().BeEquivalentTo(recipients.Select(user => user.Id));
        previous.Description.Should().Be(first.Description);
        await AssertBalancesAsync(scenario, recipients, expected: 22);
    }

    private static async Task AssertBalancesAsync(CommandsDiscordScenario scenario,
        IReadOnlyList<ScenarioUser> recipients, long expected)
    {
        foreach (var recipient in recipients)
        {
            (await scenario.State.BalanceAsync(recipient)).Should().Be(expected);
        }
    }

    [Fact]
    public async Task Reward_PaginatesBalancesAndRejectionsWithinDescriptionLimit()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var recipients = await scenario.Given.UsersAsync(count: 35, taypoints: long.MaxValue - 13);
        var rejected = Enumerable.Range(1, 100).Select(index => $"invalid-{index:D3}").ToList();
        var input = string.Join(' ', recipients.Select(user => user.Id).Concat(rejected));

        var first = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.Text("users", input)]);
        var pages = await ReadPagesAsync(scenario, owner, first);

        pages.Should().OnlyContain(page => page.Description.Length <= 4096);
        var descriptions = string.Join('\n', pages.Select(page => page.Description));
        Regex.Matches(descriptions, @"(?m)^<@(\d+)>:").Select(match => match.Groups[1].Value)
            .Should().BeEquivalentTo(recipients.Select(user => user.Id));
        Regex.Matches(descriptions, @"invalid-\d{3}").Select(match => match.Value).Should().BeEquivalentTo(rejected);
        await AssertBalancesAsync(scenario, recipients, expected: long.MaxValue);
    }

    private static async Task<IReadOnlyList<DiscordExchange>> ReadPagesAsync(CommandsDiscordScenario scenario,
        ScenarioUser owner, DiscordExchange first)
    {
        var footer = first.Embed.GetProperty("footer").GetProperty("text").GetString()!;
        var count = int.Parse(footer.Split('/')[^1], CultureInfo.InvariantCulture);
        List<DiscordExchange> pages = [first];
        for (var index = 1; index < count; index++)
        {
            pages.Add(await scenario.Discord.ClickAsync(owner, pages[^1], "Next"));
        }
        return pages;
    }

    [Fact]
    public async Task Reward_FitsMoreThanNineUsersInOneEmbedWithoutPaging()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var recipients = await scenario.Given.UsersAsync(count: 15, taypoints: 9);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.Text("users", string.Join(' ', recipients.Select(user => user.Id)))]);

        response.ShouldBeSuccess();
        Regex.Matches(response.Description, @"(?m)^<@(\d+)>:").Select(match => match.Groups[1].Value)
            .Should().BeEquivalentTo(recipients.Select(user => user.Id));
        response.Message.GetProperty("components").EnumerateArray().Should().BeEmpty();
        await AssertBalancesAsync(scenario, recipients, expected: 22);
    }

    [Fact]
    public async Task Reward_ReportsUnexpectedLookupFailureWithoutRewardingAnyone()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var recipient = await scenario.Given.UserAsync(taypoints: 9);
        const string unavailable = "100000000000009999";
        scenario.DiscordApi.ExpectRequest("GET", $"users/{unavailable}",
            new { code = 50001, message = "Missing Access" }, HttpStatusCode.Forbidden);
        scenario.Logs.ExpectError<SlashCommandHandler>("Unhandled exception in slash command");

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.Text("users", $"{recipient.Id} {unavailable}")]);

        response.ShouldBeError();
        (await scenario.State.BalanceAsync(recipient)).Should().Be(9);
    }

    [Fact]
    public async Task Reward_PaginatesRejectedEntriesWithoutSplittingThem()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var entries = Enumerable.Range(1, 100).Select(index => $"invalid-{index:D3}").ToList();

        var first = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.Text("users", string.Join(' ', entries))]);
        var next = await scenario.Discord.ClickAsync(owner, first, "Next");

        first.ShouldBeError();
        next.ShouldBeError();
        Regex.Matches(first.Description + next.Description, @"invalid-\d{3}").Select(match => match.Value)
            .Should().BeEquivalentTo(entries);
        first.Description.Length.Should().BeLessThanOrEqualTo(4096);
        next.Description.Length.Should().BeLessThanOrEqualTo(4096);
    }

    [Fact]
    public async Task Reward_HandlesLongestInvalidEntryWithinEmbedLimits()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.Text("users", new string('x', count: 6000))]);

        response.ShouldBeError();
        response.Description.Length.Should().BeLessThanOrEqualTo(4096);
        response.Description.Should().Contain("invalid");
    }

    [Fact]
    public async Task Reward_DeduplicatesMentionsAndIdsAndReportsRejectedEntries()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var first = await scenario.Given.UserAsync(taypoints: 9);
        var second = await scenario.Given.UserAsync(taypoints: 31);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-users", arguments:
            [SlashArgument.Integer("amount", value: 13),
                SlashArgument.Text("users", $"  <@{first.Id}>  {second.Id} {first.Id} <@!{second.Id}> invalid  ")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain(first.Id).And.Contain(second.Id).And.Contain("invalid").And.Contain("22").And.Contain("44");
        response.Description.Split('\n').Single(line => line.StartsWith($"<@{first.Id}>:", StringComparison.Ordinal))
            .Should().Contain("9").And.Contain("22").And.Contain("taypoints");
        response.Description.Split('\n').Single(line => line.StartsWith($"<@{second.Id}>:", StringComparison.Ordinal))
            .Should().Contain("31").And.Contain("44").And.Contain("taypoints");
        (await scenario.State.BalanceAsync(first)).Should().Be(22);
        (await scenario.State.BalanceAsync(second)).Should().Be(44);
    }
}
