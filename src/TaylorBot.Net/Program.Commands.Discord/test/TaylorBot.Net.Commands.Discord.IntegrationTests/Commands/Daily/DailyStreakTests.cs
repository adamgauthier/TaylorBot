using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Daily;

[Trait("Command", "daily rebuy")]
[Trait("Command", "daily streak")]
public sealed class DailyStreakTests(DataServices data)
{
    [Theory]
    [InlineData(2, 10, "highest streak ever")]
    [InlineData(10, 10, "highest it's ever been")]
    public async Task Streak_ShowsSelectedUsersCurrentAndMaximum(int current, int maximum, string expected)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var author = await scenario.Given.UserAsync();
        var user = await scenario.Given.UserAsync(username: "DailyPlayer");
        await scenario.Given.DailyRecordAsync(user, current, maximum);

        var response = await scenario.Discord.InvokeSlashCommandAsync(author, "daily streak", selectedUser: user);

        response.ShouldBeSuccess();
        response.Description.Replace("*", "", StringComparison.Ordinal).Should().Contain(user.Id).And.Contain(expected).And.Contain($"{maximum} days");
        (await scenario.State.DailyStreakAsync(user)).Should().Be(current);
    }

    [Fact]
    public async Task Streak_NoHistoryExplainsHowToStart()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "daily streak");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("never claimed").And.Contain("daily claim");
    }

    [Fact]
    public async Task Rebuy_RestoresMaximumAndDeductsCostOnlyAfterConfirmation()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 700);
        await scenario.Given.DailyRecordAsync(user);

        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "daily rebuy");
        prompt.Description.Replace("*", "", StringComparison.Ordinal).Should().Contain("500 taypoints");
        (await scenario.State.BalanceAsync(user)).Should().Be(700);
        var response = await scenario.Discord.ClickAsync(user, prompt, "Confirm");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("reset your daily streak");
        (await scenario.State.BalanceAsync(user)).Should().Be(200);
        (await scenario.State.DailyStreakAsync(user)).Should().Be(10);
    }

    [Fact]
    public async Task Rebuy_InsufficientFundsRollsBackStreakAndBalance()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 499);
        await scenario.Given.DailyRecordAsync(user);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "daily rebuy");

        var response = await scenario.Discord.ClickAsync(user, prompt, "Confirm");

        response.ShouldBeError();
        response.Description.Should().Contain("Could not rebuy").And.Contain("500");
        (await scenario.State.BalanceAsync(user)).Should().Be(499);
        (await scenario.State.DailyStreakAsync(user)).Should().Be(2);
    }

    [Fact]
    public async Task Rebuy_AlreadyRestoredStreakRejectsStaleConfirmation()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 700);
        await scenario.Given.DailyRecordAsync(user);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "daily rebuy");
        await scenario.Given.DailyRecordAsync(user, current: 10, maximum: 10);

        var response = await scenario.Discord.ClickAsync(user, prompt, "Confirm");

        response.ShouldBeError();
        response.Description.Should().Contain("streak has changed");
        (await scenario.State.BalanceAsync(user)).Should().Be(700);
        (await scenario.State.DailyStreakAsync(user)).Should().Be(10);
    }

    [Fact]
    public async Task Rebuy_NoDailyHistoryDoesNotOfferPurchase()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 700);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "daily rebuy");

        response.ShouldBeError();
        response.Description.Should().Contain("never claimed");
        (await scenario.State.BalanceAsync(user)).Should().Be(700);
    }

    [Fact]
    public async Task Rebuy_CurrentRecordDoesNotOfferPurchase()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 700);
        await scenario.Given.DailyRecordAsync(user, current: 10, maximum: 10);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "daily rebuy");

        response.ShouldBeError();
        response.Description.Should().Contain("nothing to buy back");
        (await scenario.State.BalanceAsync(user)).Should().Be(700);
    }

    [Fact]
    public async Task LegacyDaily_ClaimsOnceAndRejectsSecondClaim()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["DailyPayout:LegacyDailyPayoutAmount"] = "50" });
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var claimed = await scenario.Discord.SendMessageAsync(user, guild, "!daily");
        var duplicate = await scenario.Discord.SendMessageAsync(user, guild, "!daily");

        claimed.ShouldBeSuccess();
        duplicate.ShouldBeError();
        claimed.Description.Replace("*", "", StringComparison.Ordinal).Should().Contain("50 taypoints");
        duplicate.Description.Should().Contain("already redeemed");
        (await scenario.State.BalanceAsync(user)).Should().Be(50);
        (await scenario.State.DailyStreakAsync(user)).Should().Be(1);
    }

    [Fact]
    public async Task Rebuy_CancelPreservesStreakAndBalance()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 700);
        await scenario.Given.DailyRecordAsync(user);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "daily rebuy");

        var response = await scenario.Discord.ClickAsync(user, prompt, "Cancel");

        response.Description.Should().Contain("Operation cancelled");
        (await scenario.State.DailyStreakAsync(user)).Should().Be(2);
        (await scenario.State.BalanceAsync(user)).Should().Be(700);
    }
}
