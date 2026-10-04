using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Economy;

[Trait("Command", "heist profile")]
[Trait("Command", "risk profile")]
[Trait("Command", "roll profile")]
[Trait("Command", "rps profile")]
public sealed class GameProfileTests(DataServices data)
{
    [Theory]
    [InlineData("risk", "Risks Won", "Risk Profits")]
    [InlineData("heist", "Heists Won", "Heist Profits")]
    public async Task InvestmentProfile_ShowsSelectedUsersWinRateAndNetProfit(string game, string winsField, string profitsField)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var author = await scenario.Given.UserAsync();
        var user = await scenario.Given.UserAsync(username: "Investor");
        await scenario.Given.GameProfileAsync(user, game);

        var response = await scenario.Discord.InvokeSlashCommandAsync(author, $"{game} profile", selectedUser: user);

        response.ShouldBeSuccess();
        response.Embed.GetProperty("title").GetString().Should().Contain(user.Username);
        response.Field(winsField).Replace("*", "", StringComparison.Ordinal).Should().Contain("75%").And.Contain("4 total");
        response.Field(profitsField).Replace("*", "", StringComparison.Ordinal).Should().Contain("+100 taypoints").And.Contain("120").And.Contain("20");
    }

    [Theory]
    [InlineData("risk", "Risks Won", "Risk Profits")]
    [InlineData("heist", "Heists Won", "Heist Profits")]
    public async Task InvestmentProfile_NoHistoryShowsZeroNotMissingUser(string game, string winsField, string profitsField)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, $"{game} profile");

        response.Embed.GetProperty("title").GetString().Should().Contain(user.Username);
        response.Field(winsField).Replace("*", "", StringComparison.Ordinal).Should().Contain("0%").And.Contain("0 total");
        response.Field(profitsField).Replace("*", "", StringComparison.Ordinal).Should().Contain("+0 taypoints");
    }

    [Theory]
    [InlineData("roll", "30", "3 perfect", "every 10 rolls")]
    [InlineData("rps", "50%", "3 games", "2 games")]
    public async Task GameProfile_ShowsSelectedUsersStatistics(string game, string total, string wins, string extra)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var author = await scenario.Given.UserAsync();
        var user = await scenario.Given.UserAsync(username: "Player");
        await scenario.Given.GameProfileAsync(user, game);

        var response = await scenario.Discord.InvokeSlashCommandAsync(author, $"{game} profile", selectedUser: user);

        response.Embed.GetProperty("title").GetString().Should().Contain(user.Username);
        response.Description.Replace("*", "", StringComparison.Ordinal).Should().Contain(total).And.Contain(wins).And.Contain(extra);
    }

    [Theory]
    [InlineData("roll", "0 times", "100%")]
    [InlineData("rps", "0 games", "0%")]
    public async Task GameProfile_NoHistoryShowsZeroStatistics(string game, string total, string percentage)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, $"{game} profile");

        response.Embed.GetProperty("title").GetString().Should().Contain(user.Username);
        response.Description.Replace("*", "", StringComparison.Ordinal).Should().Contain(total).And.Contain(percentage);
    }

    [Theory]
    [InlineData(0, 0, 0, "0%", 0xf04747)]
    [InlineData(1, 2, 1, "25%", 0xf04747)]
    [InlineData(1, 1, 1, "33%", 0x43b581)]
    [InlineData(2, 0, 1, "67%", 0x43b581)]
    public async Task RpsProfile_ClassifiesWinRateAgainstOneThird(int wins, int draws, int losses, string winRate, int expectedColor)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.GameProfileAsync(user, "rps", wins, draws, losses);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "rps profile");

        response.Description.Replace("*", "", StringComparison.Ordinal).Should()
            .Contain($"{winRate} wins out of {wins + draws + losses} games")
            .And.Contain($"Won {wins} game").And.Contain($"Drew {draws} game").And.Contain($"Lost {losses} game");
        response.Embed.GetProperty("color").GetInt32().Should().Be(expectedColor);
    }
}
