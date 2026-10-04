using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Server;

[Trait("Command", "server names")]
[Trait("Command", "server joined")]
[Trait("Command", "server messages")]
[Trait("Command", "server minutes")]
[Trait("Command", "server timeline")]
[Trait("Command", "server leaderboard")]
public sealed class ServerActivityTests(DataServices data)
{
    [Theory]
    [InlineData("messages", "12 messages", "2.50")]
    [InlineData("minutes", "90 minutes", "1 hour")]
    [InlineData("joined", "1577923200", "first joined")]
    public async Task Activity_ShowsSelectedMembersPersistedStatistics(string command, string value, string detail)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var author = await scenario.Given.UserAsync();
        var member = await scenario.Given.UserAsync(username: "Member");
        var guild = await scenario.Given.GuildAsync(author);
        await scenario.Given.MemberAsync(guild, member);
        await scenario.Given.ServerActivityAsync(guild, member);

        var response = await scenario.Discord.InvokeSlashCommandAsync(author, $"server {command}", guild, selectedUser: member);

        response.ShouldBeSuccess();
        response.Description.Replace("*", "", StringComparison.Ordinal).Should().Contain(member.Id).And.Contain(value).And.Contain(detail);
        (await scenario.State.JoinedAsync(guild, member)).Should().Be(new DateTime(year: 2020, month: 1, day: 2, hour: 0, minute: 0, second: 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task Messages_NoMessagesShowsZeroAverage()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "server messages", guild);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("0 messages").And.Contain("0.00").And.NotContain("NaN");
    }

    [Fact]
    public async Task Joined_MissingDateIsRepairedFromDiscordMember()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.MissingJoinedDateAsync(guild, user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "server joined", guild);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("1767225600");
        (await scenario.State.JoinedAsync(guild, user)).Should().Be(new DateTime(year: 2026, month: 1, day: 1, hour: 0, minute: 0, second: 0, DateTimeKind.Utc));
    }

    [Theory]
    [InlineData("messages", "12 messages")]
    [InlineData("minutes", "90 minutes")]
    [InlineData("joined", "1577923200")]
    public async Task LegacyActivity_ResolvesMentionedMember(string command, string value)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var author = await scenario.Given.UserAsync();
        var member = await scenario.Given.UserAsync(username: "Member");
        var guild = await scenario.Given.GuildAsync(author);
        await scenario.Given.MemberAsync(guild, member);
        await scenario.Given.ServerActivityAsync(guild, member);

        var response = await scenario.Discord.SendMessageAsync(author, guild, $"!{command} <@{member.Id}>");

        response.ShouldBeSuccess();
        response.Description.Replace("*", "", StringComparison.Ordinal).Should().Contain(member.Id).And.Contain(value);
    }

    [Theory]
    [InlineData("names")]
    [InlineData("joined")]
    [InlineData("messages")]
    [InlineData("minutes")]
    [InlineData("timeline")]
    public async Task ServerRoute_RequiresGuild(string command)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, $"server {command}");

        response.ShouldBeError();
        response.Description.Should().Contain("server");
    }

    [Theory]
    [InlineData("messages")]
    [InlineData("minutes")]
    public async Task Leaderboard_RanksActivityAndNavigates(string statistic)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.EconomyLeaderboardAsync(guild, "server");
        await scenario.Given.ExcludedEconomyLeadersAsync(guild, "server");

        var first = await scenario.Discord.InvokeSlashCommandAsync(user, "server leaderboard", guild, arguments: [SlashArgument.Text("for", statistic)]);
        first.Description.Should().Contain("Rank01").And.Contain("999").And.NotContain("Rank16").And.NotContain("Departed").And.NotContain("Outsider");
        var next = await scenario.Discord.ClickAsync(user, first, "Next");

        next.ShouldBeSuccess();
        next.Description.Should().Contain("Rank16").And.Contain("984").And.NotContain("Rank01");
    }

    [Theory]
    [InlineData("messages")]
    [InlineData("minutes")]
    public async Task Leaderboard_NoActivityShowsEmptyState(string statistic)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "server leaderboard", guild, arguments: [SlashArgument.Text("for", statistic)]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("No data found");
    }

    [Fact]
    public async Task Names_ShowsMostRecentFirstAndNavigatesToOldest()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.ServerNamesAsync(guild);

        var first = await scenario.Discord.InvokeSlashCommandAsync(user, "server names", guild);
        first.Description.Should().Contain("Previous name 16").And.NotContain("Previous name 01");
        var next = await scenario.Discord.ClickAsync(user, first, "Next");

        next.ShouldBeSuccess();
        next.Description.Should().Contain("Previous name 01").And.NotContain("Previous name 16");
    }

    [Fact]
    public async Task Names_NoHistoryExplainsMissingRecords()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "server names", guild);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("No server name recorded");
    }

    [Fact]
    public async Task Timeline_RanksChronologicalJoinsAndNavigates()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.ServerActivityAsync(guild, user);
        await scenario.Given.EconomyLeaderboardAsync(guild, "server");

        var first = await scenario.Discord.InvokeSlashCommandAsync(user, "server timeline", guild);
        first.Description.Should().Contain("Rank01").And.Contain("1st to join").And.NotContain("Rank16");
        var next = await scenario.Discord.ClickAsync(user, first, "Next");

        next.ShouldBeSuccess();
        next.Description.Should().Contain("Rank16").And.Contain("17th to join");
    }

    [Fact]
    public async Task LegacyMinutes_HistoricalServerIncludesOldOnlineStatusCount()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user, id: "115332333745340416");
        await scenario.Given.ServerActivityAsync(guild, user);
        await scenario.Given.OldMinutesAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!minutes");

        response.ShouldBeSuccess();
        response.Description.Replace("*", "", StringComparison.Ordinal).Should().Contain("90 minutes").And.Contain("123 minutes").And.Contain("December 25th 2015");
    }

    [Fact]
    public async Task Leaderboard_DirectMessageRequiresServer()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "server leaderboard", arguments: [SlashArgument.Text("for", "messages")]);

        response.ShouldBeError();
        response.Description.Should().Contain("server");
    }
}
