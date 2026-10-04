using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Taypoints;

[Trait("Command", "taypoints succession")]
public sealed class SuccessionTests(DataServices data)
{
    [Fact]
    public async Task Show_NoWillOffersSuccessorSelection()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints succession");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("NOT safe");
        response.Message.GetProperty("components").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task Show_ExistingWillShowsSuccessorAndRemoval()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var successor = await scenario.Given.UserAsync(username: "Successor");
        await scenario.Given.WillAsync(user, successor);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints succession");

        response.ShouldBeSuccess();
        response.Description.Should().Contain(successor.Id).And.Contain(successor.Username);
        response.Message.GetProperty("components").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task Select_SelfIsRejectedWithoutCreatingWill()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints succession");

        var response = await scenario.Discord.SelectUserAsync(user, prompt, user);

        response.ShouldBeError();
        response.Description.Should().Contain("yourself");
        (await scenario.State.SuccessorAsync(user)).Should().BeNull();
    }

    [Fact]
    public async Task Select_AnotherUserPersistsSuccessor()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var successor = await scenario.Given.UserAsync(username: "Successor");
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints succession");

        var response = await scenario.Discord.SelectUserAsync(user, prompt, successor);

        response.ShouldBeSuccess();
        response.Description.Should().Contain(successor.Id);
        (await scenario.State.SuccessorAsync(user)).Should().Be(successor.Id);
    }

    [Fact]
    public async Task Remove_ClearsExistingWill()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var successor = await scenario.Given.UserAsync();
        await scenario.Given.WillAsync(user, successor);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints succession");

        var response = await scenario.Discord.ClickAsync(user, prompt, "Remove Successor");

        response.ShouldBeSuccess();
        (await scenario.State.SuccessorAsync(user)).Should().BeNull();
    }

    [Fact]
    public async Task Claim_TransfersInactiveOwnersBalanceAndRemovesWill()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(taypoints: 100, username: "Owner");
        var beneficiary = await scenario.Given.UserAsync(taypoints: 13);
        await scenario.Given.WillAsync(owner, beneficiary, inactiveDays: 21);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(beneficiary, "taypoints succession");
        prompt.Description.Should().Contain("Available to Claim").And.Contain(owner.Id);
        prompt.Message.GetProperty("components")[0].GetProperty("components").GetArrayLength().Should().Be(2);

        var response = await scenario.Discord.ClickAsync(beneficiary, prompt, "Claim");

        response.ShouldBeSuccess();
        (await scenario.State.BalanceAsync(beneficiary)).Should().Be(113);
        (await scenario.State.BalanceAsync(owner)).Should().Be(0);
        (await scenario.State.SuccessorAsync(owner)).Should().BeNull();
    }

    [Fact]
    public async Task Claim_RechecksActivityBeforeTransferring()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(taypoints: 100);
        var beneficiary = await scenario.Given.UserAsync();
        await scenario.Given.WillAsync(owner, beneficiary);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(beneficiary, "taypoints succession");
        await scenario.Given.ActiveTodayAsync(owner);

        var response = await scenario.Discord.ClickAsync(beneficiary, prompt, "Claim");

        response.ShouldBeError();
        (await scenario.State.BalanceAsync(owner)).Should().Be(100);
        (await scenario.State.BalanceAsync(beneficiary)).Should().Be(0);
        (await scenario.State.SuccessorAsync(owner)).Should().Be(beneficiary.Id);
    }
}
