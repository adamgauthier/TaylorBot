using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Taypoints;

[Trait("Command", "taypoints gift")]
public sealed class TaypointsGiftTests(DataServices data)
{
    [Fact]
    public async Task Gift_TransfersPointsAndRefreshesGuildBalances()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var sender = await scenario.Given.UserAsync(taypoints: 100);
        var recipient = await scenario.Given.UserAsync(taypoints: 13, username: "Recipient");
        var guild = await scenario.Given.GuildAsync(sender);
        await scenario.Given.MemberAsync(guild, recipient);

        var response = await scenario.Discord.InvokeSlashCommandAsync(sender, "taypoints gift", guild, selectedUser: recipient, arguments: [SlashArgument.Text("amount", "40")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Taypoint Transfer").And.Contain(sender.Id).And.Contain(recipient.Id);
        (await scenario.State.BalanceAsync(sender)).Should().Be(60);
        (await scenario.State.BalanceAsync(recipient)).Should().Be(53);
        (await scenario.State.LastKnownTaypointsAsync(guild, sender)).Should().Be(60);
        (await scenario.State.LastKnownTaypointsAsync(guild, recipient)).Should().Be(53);
    }

    [Theory]
    [InlineData("1000", 1000, 1000)]
    [InlineData("half", 1000, 1000)]
    [InlineData("all", 0, 2000)]
    public async Task Gift_LargeTransferRequiresConfirmation(string amount, long remaining, long received)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var sender = await scenario.Given.UserAsync(taypoints: 2000);
        var recipient = await scenario.Given.UserAsync(username: "Recipient");
        var guild = await scenario.Given.GuildAsync(sender);
        await scenario.Given.MemberAsync(guild, recipient);

        var prompt = await scenario.Discord.InvokeSlashCommandAsync(sender, "taypoints gift", guild, selectedUser: recipient, arguments: [SlashArgument.Text("amount", amount)]);
        prompt.Description.Should().Contain("Are you sure");
        (await scenario.State.BalanceAsync(sender)).Should().Be(2000);
        var response = await scenario.Discord.ClickAsync(sender, prompt, "Confirm");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Balances Updated");
        (await scenario.State.BalanceAsync(sender)).Should().Be(remaining);
        (await scenario.State.BalanceAsync(recipient)).Should().Be(received);
    }

    [Theory]
    [InlineData("0", "higher than 0")]
    [InlineData("-1", "higher than 0")]
    [InlineData("101", "only have")]
    [InlineData("banana", "valid number")]
    public async Task Gift_InvalidAmountLeavesBalancesUnchanged(string amount, string error)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var sender = await scenario.Given.UserAsync(taypoints: 100);
        var recipient = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(sender, "taypoints gift", selectedUser: recipient, arguments: [SlashArgument.Text("amount", amount)]);

        response.ShouldBeError();
        response.Description.Should().Contain(error);
        (await scenario.State.BalanceAsync(sender)).Should().Be(100);
        (await scenario.State.BalanceAsync(recipient)).Should().Be(0);
    }

    [Fact]
    public async Task Gift_CannotSendToYourself()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var sender = await scenario.Given.UserAsync(taypoints: 100);

        var response = await scenario.Discord.InvokeSlashCommandAsync(sender, "taypoints gift", selectedUser: sender, arguments: [SlashArgument.Text("amount", "10")]);

        response.ShouldBeError();
        response.Description.Should().Contain("yourself");
        (await scenario.State.BalanceAsync(sender)).Should().Be(100);
    }

    [Fact]
    public async Task Confirm_AnotherUserCannotApproveTransfer()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var sender = await scenario.Given.UserAsync(taypoints: 2000);
        var recipient = await scenario.Given.UserAsync();
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(sender, "taypoints gift", selectedUser: recipient, arguments: [SlashArgument.Text("amount", "all")]);

        var response = await scenario.Discord.ClickAsync(recipient, prompt, "Confirm");

        response.Requests.Should().ContainSingle().Which.Body!.Value.GetProperty("type").GetInt32().Should().Be(6);
        (await scenario.State.BalanceAsync(sender)).Should().Be(2000);
        (await scenario.State.BalanceAsync(recipient)).Should().Be(0);
    }

    [Fact]
    public async Task LegacyGift_DividesAmountAcrossRecipientsWithRemainderFirst()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var sender = await scenario.Given.UserAsync(taypoints: 100);
        var first = await scenario.Given.UserAsync(username: "First");
        var second = await scenario.Given.UserAsync(username: "Second");
        var guild = await scenario.Given.GuildAsync(sender);
        await scenario.Given.MemberAsync(guild, first);
        await scenario.Given.MemberAsync(guild, second);

        var response = await scenario.Discord.SendMessageAsync(sender, guild, $"!gift 13 <@{first.Id}> <@{second.Id}>");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("multiple users");
        (await scenario.State.BalanceAsync(sender)).Should().Be(87);
        (await scenario.State.BalanceAsync(first)).Should().Be(7);
        (await scenario.State.BalanceAsync(second)).Should().Be(6);
    }

    [Fact]
    public async Task Gift_ToBotWarnsBeforeIrreversibleTransfer()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var sender = await scenario.Given.UserAsync(taypoints: 100);
        var guild = await scenario.Given.GuildAsync(sender);
        var bot = guild.Members.Single(member => member.Id == DiscordApi.ApplicationId);

        var prompt = await scenario.Discord.InvokeSlashCommandAsync(sender, "taypoints gift", guild, selectedUser: bot, arguments: [SlashArgument.Text("amount", "10")]);
        prompt.Description.Should().Contain("Bots can").And.Contain("lost");
        (await scenario.State.BalanceAsync(sender)).Should().Be(100);
        var response = await scenario.Discord.ClickAsync(sender, prompt, "Confirm");

        response.ShouldBeSuccess();
        (await scenario.State.BalanceAsync(sender)).Should().Be(90);
        (await scenario.State.BalanceAsync(bot)).Should().Be(10);
    }

    [Fact]
    public async Task Gift_CancelDoesNotTransfer()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var sender = await scenario.Given.UserAsync(taypoints: 100);
        var recipient = await scenario.Given.UserAsync();
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(sender, "taypoints gift", selectedUser: recipient, arguments: [SlashArgument.Text("amount", "all")]);

        var response = await scenario.Discord.ClickAsync(sender, prompt, "Cancel");

        response.Description.Should().Contain("Operation cancelled");
        (await scenario.State.BalanceAsync(sender)).Should().Be(100);
        (await scenario.State.BalanceAsync(recipient)).Should().Be(0);
    }

    [Fact]
    public async Task Confirm_RecalculatesRelativeAmountFromCurrentBalance()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var sender = await scenario.Given.UserAsync(taypoints: 2000);
        var recipient = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(sender);
        await scenario.Given.MemberAsync(guild, recipient);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(sender, "taypoints gift", guild, selectedUser: recipient, arguments: [SlashArgument.Text("amount", "half")]);
        await scenario.Given.TaypointBalanceAsync(sender, balance: 100);

        var response = await scenario.Discord.ClickAsync(sender, prompt, "Confirm");

        response.ShouldBeSuccess();
        (await scenario.State.BalanceAsync(sender)).Should().Be(50);
        (await scenario.State.BalanceAsync(recipient)).Should().Be(50);
    }
}
