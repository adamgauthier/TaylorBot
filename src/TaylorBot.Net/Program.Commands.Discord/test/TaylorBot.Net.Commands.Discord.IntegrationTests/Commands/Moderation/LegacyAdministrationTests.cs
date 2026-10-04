using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Moderation;

public sealed class LegacyAdministrationTests(DataServices data)
{
    [Fact]
    public async Task Reward_MentionListCreditsAllRecipients()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var first = await scenario.Given.UserAsync(taypoints: 3);
        var second = await scenario.Given.UserAsync(taypoints: 7);
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.MemberAsync(guild, first);
        await scenario.Given.MemberAsync(guild, second);

        var response = await scenario.Discord.SendMessageAsync(owner, guild, $"!reward 13 <@{first.Id}> <@{second.Id}>");

        response.ShouldBeSuccess();
        response.Description.Should().Contain(first.Id).And.Contain(second.Id);
        (await scenario.State.BalanceAsync(first)).Should().Be(16);
        (await scenario.State.BalanceAsync(second)).Should().Be(20);
    }

    [Fact]
    public async Task Reward_NonOwnerCannotCreditMentionedRecipient()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var recipient = await scenario.Given.UserAsync(taypoints: 7);
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.MemberAsync(guild, recipient);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!reward 13 <@{recipient.Id}>");

        response.ShouldBeError();
        response.Description.Should().Contain("owner");
        (await scenario.State.BalanceAsync(recipient)).Should().Be(7);
    }

    [Theory]
    [InlineData("!command disable-global avatar Maintenance")]
    [InlineData("!command enable-global avatar")]
    public async Task GlobalCommandConfiguration_NonOwnerCannotChangeDisabledState(string message)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.DisableCommandAsync("avatar", "Original reason");

        var response = await scenario.Discord.SendMessageAsync(user, guild, message);

        response.ShouldBeError();
        response.Description.Should().Contain("owner");
        (await scenario.State.DisabledReasonAsync("avatar")).Should().Be("Original reason");
    }
}
