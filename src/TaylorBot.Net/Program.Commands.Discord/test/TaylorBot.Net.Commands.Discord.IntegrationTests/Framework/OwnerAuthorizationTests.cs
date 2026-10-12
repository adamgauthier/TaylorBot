using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Framework;

[Trait("Command", "owner reward-user")]
public sealed class OwnerAuthorizationTests(DataServices data)
{
    [Fact]
    public async Task TeamOwner_CanRunOwnerCommands()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api => api.SetApplicationTeam(ownerId: "100000000000000101", memberId: "100000000000000102"));
        var owner = await scenario.Given.UserAsync();
        var recipient = await scenario.Given.UserAsync(taypoints: 9);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward-user",
            arguments: [SlashArgument.Integer("amount", value: 1), SlashArgument.User("user", recipient)]);

        response.ShouldBeSuccess();
        (await scenario.State.BalanceAsync(recipient)).Should().Be(10);
    }

    [Fact]
    public async Task TeamAdmin_CannotRunOwnerCommands()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api => api.SetApplicationTeam(ownerId: "100000000000000101", memberId: "100000000000000102"));
        await scenario.Given.UserAsync();
        var member = await scenario.Given.UserAsync();
        var recipient = await scenario.Given.UserAsync(taypoints: 9);

        var response = await scenario.Discord.InvokeSlashCommandAsync(member, "owner reward-user",
            arguments: [SlashArgument.Integer("amount", value: 1), SlashArgument.User("user", recipient)]);

        response.ShouldBeError();
        response.Description.Should().Contain("owner");
        (await scenario.State.BalanceAsync(recipient)).Should().Be(9);
    }

    [Fact]
    public async Task TeamApplicationPlaceholder_CannotRunOwnerCommands()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api => api.SetApplicationTeam(ownerId: "100000000000000101", memberId: "100000000000000102"));
        var placeholder = await scenario.Given.UserAsync(botOwner: true);
        var recipient = await scenario.Given.UserAsync(taypoints: 9);

        var response = await scenario.Discord.InvokeSlashCommandAsync(placeholder, "owner reward-user",
            arguments: [SlashArgument.Integer("amount", value: 1), SlashArgument.User("user", recipient)]);

        response.ShouldBeError();
        response.Description.Should().Contain("owner");
        (await scenario.State.BalanceAsync(recipient)).Should().Be(9);
    }
}
