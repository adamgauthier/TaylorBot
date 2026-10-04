using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Taypoints;

[Trait("Command", "taypoints balance")]
public sealed class TaypointsBalanceTests(DataServices data)
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1234)]
    public async Task BalanceInDm_ShowsCurrentTaypoints(long amount)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(amount);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");

        response.ShouldShowBalance(user, taypoints: amount);
    }

    [Fact]
    public async Task BalanceInGuild_ShowsRankAndUpdatesCachedBalance()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 1_234);
        var guild = await scenario.Given.GuildAsync(user, cachedTaypoints: 13);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance", guild);

        response.ShouldShowBalance(user, taypoints: 1_234, rank: 1);
        (await scenario.State.LastKnownTaypointsAsync(guild, user)).Should().Be(1_234);
    }

    [Fact]
    public async Task BalanceForAnotherUser_ShowsSelectedUsersBalance()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var author = await scenario.Given.UserAsync(taypoints: 13);
        var other = await scenario.Given.UserAsync(taypoints: 1_989, username: "Bob");

        var response = await scenario.Discord.InvokeSlashCommandAsync(author, "taypoints balance", selectedUser: other);

        response.ShouldShowBalance(other, taypoints: 1_989);
    }
}
