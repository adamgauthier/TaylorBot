using System.Globalization;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Economy;

[Trait("Command", "risk play")]
[Trait("Command", "roll play")]
[Trait("Command", "rps play")]
public sealed class GamePlayTests(DataServices data)
{
    [Theory]
    [InlineData("low", 1)]
    [InlineData("moderate", 3)]
    [InlineData("high", 9)]
    public async Task Risk_RecordsOneOutcomeAndAppliesLevelPayout(string level, int multiplier)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 100);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "risk play", arguments: [SlashArgument.Text("amount", "10"), SlashArgument.Text("level", level)]);

        var state = await scenario.State.GameAsync(user, "risk");
        state.Played.Should().Be(1);
        state.Won.Should().Be(state.Wins * 10 * multiplier);
        state.Lost.Should().Be(state.Losses * 10);
        (await scenario.State.BalanceAsync(user)).Should().Be(100 + state.Won - state.Lost);
        response.Description.Should().Contain($"{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(level)} Risk").And.Contain("10 taypoints").And.Contain("10% of balance");
        response.Embed.GetProperty("color").GetInt32().Should().Be(state.Wins == 1 ? 0x43b581 : 0xf04747);
    }

    [Theory]
    [InlineData("gamble", "Low", 1)]
    [InlineData("supergamble", "High", 9)]
    public async Task LegacyRisk_UsesDistinctRiskLevelAndRelativeAmount(string command, string level, int multiplier)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 100);
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!{command} half");

        var state = await scenario.State.GameAsync(user, "risk");
        state.Played.Should().Be(1);
        state.Won.Should().Be(state.Wins * 50 * multiplier);
        state.Lost.Should().Be(state.Losses * 50);
        (await scenario.State.BalanceAsync(user)).Should().Be(100 + state.Won - state.Lost);
        response.Description.Should().Contain($"{level} Risk").And.Contain("50 taypoints");
    }

    [Theory]
    [InlineData("0", "higher than 0")]
    [InlineData("101", "only have")]
    [InlineData("unknown", "valid number")]
    public async Task Risk_InvalidInvestmentDoesNotPlay(string amount, string error)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 100);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "risk play", arguments: [SlashArgument.Text("amount", amount)]);

        response.ShouldBeError();
        response.Description.Should().Contain(error);
        (await scenario.State.BalanceAsync(user)).Should().Be(100);
        (await scenario.State.GameAsync(user, "risk")).Played.Should().Be(0);
    }

    [Theory]
    [InlineData("rock", "🪨")]
    [InlineData("paper", "📄")]
    [InlineData("scissors", "✂️")]
    public async Task Rps_RecordsExactlyOneOutcomeAndRewardsOnlyWins(string shape, string emoji)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 100);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "rps play", arguments: [SlashArgument.Text("option", shape)]);

        var state = await scenario.State.GameAsync(user, "rps");
        state.Played.Should().Be(1);
        (state.Wins + state.Draws + state.Losses).Should().Be(1);
        response.Description.Should().Contain($"You {emoji}").And.Contain(state.Wins == 1 ? "You win!" : state.Draws == 1 ? "It's a tie!" : "You lost!");
        (await scenario.State.BalanceAsync(user)).Should().BeOneOf(100 + state.Wins, 100 + 2 * state.Wins);
    }

    [Fact]
    public async Task Rps_AutomaticShapeStillRecordsOutcome()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "rps play");

        var state = await scenario.State.GameAsync(user, "rps");
        state.Played.Should().Be(1);
        response.Description.Should().Contain("TaylorBot").And.Contain(state.Wins == 1 ? "You win!" : state.Draws == 1 ? "It's a tie!" : "You lost!");
        (await scenario.State.BalanceAsync(user)).Should().BeOneOf(state.Wins, 2 * state.Wins);
    }

    [Fact]
    public async Task LegacyRps_InvalidShapeDoesNotPlay()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 100);
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!rps lizard");

        response.ShouldBeError();
        response.Description.Should().Contain("option");
        (await scenario.State.GameAsync(user, "rps")).Played.Should().Be(0);
        (await scenario.State.BalanceAsync(user)).Should().Be(100);
    }

    [Fact]
    public async Task LegacyRps_ParsesShapeAndRecordsResult()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!rps rock");

        var state = await scenario.State.GameAsync(user, "rps");
        state.Played.Should().Be(1);
        response.Description.Should().Contain("You 🪨").And.Contain(state.Wins == 1 ? "You win!" : state.Draws == 1 ? "It's a tie!" : "You lost!");
        (await scenario.State.BalanceAsync(user)).Should().BeOneOf(state.Wins, 2 * state.Wins);
    }

    [Fact]
    public async Task Roll_RecordsDisplayedRollAndCreditsMatchingReward()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "roll play");

        var roll = int.Parse(new string(response.Description.Split('\n')[0].Where(char.IsAsciiDigit).ToArray()), CultureInfo.InvariantCulture);
        var reward = RollReward(roll);
        var state = await scenario.State.GameAsync(user, "roll");
        roll.Should().BeInRange(0, 1989);
        state.Played.Should().Be(1);
        state.Wins.Should().Be(roll == 1989 ? 1 : 0);
        (await scenario.State.BalanceAsync(user)).Should().BeOneOf(reward, 2 * reward);
        response.Description.Should().Contain(reward == 0 ? "Better luck next time!" : "You won");
    }

    [Fact]
    public async Task LegacyRoll_IgnoresRemainderAndPlaysOnce()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!roll ignored text");

        var roll = int.Parse(new string(response.Description.Split('\n')[0].Where(char.IsAsciiDigit).ToArray()), CultureInfo.InvariantCulture);
        var reward = RollReward(roll);
        (await scenario.State.GameAsync(user, "roll")).Played.Should().Be(1);
        (await scenario.State.BalanceAsync(user)).Should().BeOneOf(reward, 2 * reward);
    }

    [Theory]
    [InlineData("roll")]
    [InlineData("rps")]
    public async Task DailyLimit_RejectsGameWithoutChangingStatsOrBalance(string game)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 100);
        await scenario.Given.ExhaustGameLimitAsync(user, game);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, $"{game} play");

        response.ShouldBeError();
        response.Description.Should().Contain("limit");
        (await scenario.State.GameAsync(user, game)).Played.Should().Be(0);
        (await scenario.State.BalanceAsync(user)).Should().Be(100);
    }

    private static long RollReward(int roll) => roll switch
    {
        1989 => 5000,
        1 or 7 or 13 or 15 or 22 or 429 or 709 or 1213 => 100,
        _ => 0,
    };
}
