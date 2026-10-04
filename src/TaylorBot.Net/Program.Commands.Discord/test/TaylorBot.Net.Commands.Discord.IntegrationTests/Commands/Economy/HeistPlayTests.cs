using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Economy;

[Trait("Command", "heist play")]
public sealed class HeistPlayTests(DataServices data)
{
    [Theory]
    [InlineData(1, 120, 1, 0, "success")]
    [InlineData(101, 90, 0, 1, "failure")]
    public async Task Play_ResolvesBankBoundaryAndPersistsOutcome(int minimumRoll, long balance, long wins, long losses, string outcome)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 100);
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.HeistBankAsync(minimumRoll);
        scenario.DiscordApi.ExpectHeistResult(guild);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "heist play", guild, arguments: [SlashArgument.Text("amount", "10")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Heist started").And.Contain(user.Id);
        scenario.DiscordApi.HeistResult(guild).Should().Contain($"heist was a {outcome}").And.Contain("Integration Vault").And.Contain(user.Id);
        var state = await scenario.State.GameAsync(user, "heist");
        state.Played.Should().Be(1);
        state.Wins.Should().Be(wins);
        state.Losses.Should().Be(losses);
        state.Won.Should().Be(wins * 20);
        state.Lost.Should().Be(losses * 10);
        (await scenario.State.BalanceAsync(user)).Should().Be(balance);
        (await scenario.State.HasOpenHeistAsync(guild)).Should().BeFalse();
    }

    [Fact]
    public async Task Play_JoinsOpenHeistWithoutTakingInvestmentEarly()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 100);
        var starter = await scenario.Given.UserAsync(taypoints: 100);
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.OpenHeistAsync(guild, starter);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "heist play", guild, arguments: [SlashArgument.Text("amount", "20")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("joined the heist");
        (await scenario.State.HeistInvestmentAsync(guild, user)).Should().Be(20);
        (await scenario.State.HeistInvestmentAsync(guild, starter)).Should().Be(10);
        (await scenario.State.BalanceAsync(user)).Should().Be(100);
        (await scenario.State.GameAsync(user, "heist")).Played.Should().Be(0);
    }

    [Fact]
    public async Task LegacyHeist_UpdatesExistingInvestment()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 100);
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.OpenHeistAsync(guild, user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!heist 25");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("investment for the heist has been updated");
        (await scenario.State.HeistInvestmentAsync(guild, user)).Should().Be(25);
        (await scenario.State.BalanceAsync(user)).Should().Be(100);
    }

    [Fact]
    public async Task Play_RequiresServerAndDoesNotSpend()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 100);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "heist play", arguments: [SlashArgument.Text("amount", "10")]);

        response.ShouldBeError();
        response.Description.Should().Contain("server");
        (await scenario.State.BalanceAsync(user)).Should().Be(100);
        (await scenario.State.GameAsync(user, "heist")).Played.Should().Be(0);
    }

    [Fact]
    public async Task Play_RejectsUnaffordableInvestmentWithoutStartingHeist()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 100);
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "heist play", guild, arguments: [SlashArgument.Text("amount", "101")]);

        response.ShouldBeError();
        response.Description.Should().Contain("only have");
        (await scenario.State.BalanceAsync(user)).Should().Be(100);
        (await scenario.State.HasOpenHeistAsync(guild)).Should().BeFalse();
    }

    [Fact]
    public async Task Play_ExhaustedDailyLimitDoesNotStartHeist()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 100);
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.ExhaustGameLimitAsync(user, "heist");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "heist play", guild, arguments: [SlashArgument.Text("amount", "10")]);

        response.ShouldBeError();
        response.Description.Should().Contain("limit");
        (await scenario.State.BalanceAsync(user)).Should().Be(100);
        (await scenario.State.HasOpenHeistAsync(guild)).Should().BeFalse();
    }
}
