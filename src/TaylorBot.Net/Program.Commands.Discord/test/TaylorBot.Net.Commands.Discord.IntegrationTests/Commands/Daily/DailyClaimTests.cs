using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Daily;

[Trait("Command", "daily claim")]
public sealed class DailyClaimTests(DataServices data)
{
    [Theory]
    [InlineData(5, 13, 15)]
    [InlineData(5, 15, 20)]
    [InlineData(2, 1, 2)]
    public async Task Claim_ShowsNextBonusAndCreditsBalance(int interval, int streak, int nextBonus)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken, dailyBonusInterval: interval);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.DailyStreakAsync(user, previousStreak: streak - 1);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "daily claim");

        response.ShouldBeSuccess();
        response.Description.Replace("*", "", StringComparison.Ordinal).Should().Contain($"Streak: {streak}/{nextBonus}");
        (await scenario.State.BalanceAsync(user)).Should().Be(100);
    }
}
