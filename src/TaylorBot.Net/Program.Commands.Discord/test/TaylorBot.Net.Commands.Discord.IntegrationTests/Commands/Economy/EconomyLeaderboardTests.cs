using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Economy;

[Trait("Command", "taypoints leaderboard")]
[Trait("Command", "daily leaderboard")]
[Trait("Command", "heist leaderboard")]
[Trait("Command", "risk leaderboard")]
[Trait("Command", "roll leaderboard")]
[Trait("Command", "rps leaderboard")]
public sealed class EconomyLeaderboardTests(DataServices data)
{
    [Theory]
    [InlineData("taypoints")]
    [InlineData("daily")]
    [InlineData("risk")]
    [InlineData("heist")]
    [InlineData("roll")]
    [InlineData("rps")]
    public async Task Leaderboard_RanksMembersAndNavigatesPages(string feature)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.EconomyLeaderboardAsync(guild, feature);
        await scenario.Given.ExcludedEconomyLeadersAsync(guild, feature);

        var first = await scenario.Discord.InvokeSlashCommandAsync(user, $"{feature} leaderboard", guild);
        first.ShouldBeSuccess();
        first.Description.Should().Contain("Rank01").And.Contain("999").And.Contain("Rank15")
            .And.NotContain("Rank16").And.NotContain("Outsider").And.NotContain("Departed");
        first.Description.IndexOf("Rank01", StringComparison.Ordinal).Should().BeLessThan(first.Description.IndexOf("Rank15", StringComparison.Ordinal));
        var next = await scenario.Discord.ClickAsync(user, first, "Next");
        next.Description.Should().Contain("Rank16").And.Contain("984").And.NotContain("Rank01");
        var previous = await scenario.Discord.ClickAsync(user, next, "Previous");

        previous.Description.Should().Be(first.Description);
    }

    [Theory]
    [InlineData("daily", "No")]
    [InlineData("risk", "No")]
    [InlineData("heist", "No")]
    [InlineData("roll", "No")]
    [InlineData("rps", "No")]
    public async Task Leaderboard_EmptyGuildExplainsMissingRecords(string feature, string expected)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, $"{feature} leaderboard", guild);

        response.ShouldBeSuccess();
        response.Description.Should().Contain(expected);
        response.Description.Should().NotContain(user.Username);
    }

    [Fact]
    public async Task TaypointsLeaderboard_IncludesMembersWithZeroBalance()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints leaderboard", guild);

        response.ShouldBeSuccess();
        response.Description.Replace("*", "", StringComparison.Ordinal).Should().Contain(user.Username).And.Contain("0 taypoints");
    }

    [Theory]
    [InlineData("taypoints")]
    [InlineData("daily")]
    [InlineData("risk")]
    [InlineData("heist")]
    [InlineData("roll")]
    [InlineData("rps")]
    public async Task Leaderboard_RequiresServer(string feature)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, $"{feature} leaderboard");

        response.ShouldBeError();
        response.Description.Should().Contain("server");
    }

    [Fact]
    public async Task TaypointsLeaderboard_RefreshesStaleCachedBalanceAfterDisplayingSnapshot()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 200);
        var guild = await scenario.Given.GuildAsync(user, cachedTaypoints: 100);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints leaderboard", guild);

        response.Description.Should().Contain(user.Username).And.Contain("100");
        (await scenario.State.LastKnownTaypointsAsync(guild, user)).Should().Be(200);
        (await scenario.State.BalanceAsync(user)).Should().Be(200);
    }

    [Fact]
    public async Task Leaderboard_CancelDeletesInteractiveMessage()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "daily leaderboard", guild);

        var response = await scenario.Discord.ClickAsync(user, prompt, "Cancel");

        response.ShouldBeDeleted();
    }
}
