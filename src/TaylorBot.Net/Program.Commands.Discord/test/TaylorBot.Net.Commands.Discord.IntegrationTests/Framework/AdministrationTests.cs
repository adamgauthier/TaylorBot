using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Framework;

[Trait("Command", "avatar")]
public sealed class AdministrationTests(DataServices data)
{
    [Fact]
    public async Task Prefix_PersistsNewPrefixAndUsesItForSubsequentCommands()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!prefix .");
        var next = await scenario.Discord.SendMessageAsync(user, guild, ".choose Fearless");

        response.ShouldBeSuccess();
        (await scenario.State.PrefixAsync(guild)).Should().Be(".");
        next.Description.Should().StartWith("Fearless");
    }

    [Fact]
    public async Task EnableGlobal_ClearsCommandDisabledReason()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.DisableCommandAsync("avatar", "Maintenance");

        var response = await scenario.Discord.SendMessageAsync(owner, guild, "!command enable-global avatar");

        response.ShouldBeSuccess();
        (await scenario.State.DisabledReasonAsync("avatar")).Should().BeEmpty();
    }

    [Fact]
    public async Task DisableGlobal_FrameworkCommandCannotBeDisabled()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.KnownCommandAsync("command enable-global");

        var response = await scenario.Discord.SendMessageAsync(owner, guild, "!command disable-global \"command enable-global\" Maintenance");

        response.ShouldBeError();
        response.Description.Should().Contain("framework command");
        (await scenario.State.DisabledReasonAsync("command enable-global")).Should().BeEmpty();
    }

    [Fact]
    public async Task DisableGlobal_PersistsReasonAndRejectsCommand()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.KnownCommandAsync("avatar");

        var response = await scenario.Discord.SendMessageAsync(owner, guild, "!command disable-global avatar Maintenance");
        var rejected = await scenario.Discord.InvokeSlashCommandAsync(owner, "avatar");

        response.ShouldBeSuccess();
        (await scenario.State.DisabledReasonAsync("avatar")).Should().Be("Maintenance");
        rejected.Description.Should().Contain("Maintenance");
    }

    [Fact]
    public async Task Reward_CreditsSelectedUserAndReportsNewBalance()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var recipient = await scenario.Given.UserAsync(taypoints: 9);
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.MemberAsync(guild, recipient);

        var response = await scenario.Discord.SendMessageAsync(owner, guild, $"!reward 13 <@{recipient.Id}>");

        response.ShouldBeSuccess();
        response.Description.Split('\n').Should().Contain(line => line.Contains(recipient.Id, StringComparison.Ordinal) && line.Contains("22", StringComparison.Ordinal));
        (await scenario.State.BalanceAsync(recipient)).Should().Be(22);
    }
}
