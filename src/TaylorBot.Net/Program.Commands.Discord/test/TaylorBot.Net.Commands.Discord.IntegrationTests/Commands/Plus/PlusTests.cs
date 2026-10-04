using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Plus;

[Trait("Command", "plus show")]
[Trait("Command", "plus add")]
[Trait("Command", "plus remove")]
public sealed class PlusTests(DataServices data)
{
    [Fact]
    public async Task Add_RejectsMembershipAtServerLimit()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var first = await scenario.Given.GuildAsync(user);
        var second = await scenario.Given.GuildAsync(user);
        var target = await scenario.Given.GuildAsync(user);
        await scenario.Given.PlusAsync(user, maximumGuilds: 2, first, second);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "plus add", target);

        response.ShouldBeError();
        response.Description.Should().Contain("can't add more");
        (await scenario.State.PlusGuildStateAsync(target, user)).Should().BeNull();
    }

    [Fact]
    public async Task Add_ActivatesServerBelowMembershipLimit()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var existing = await scenario.Given.GuildAsync(user);
        var target = await scenario.Given.GuildAsync(user);
        await scenario.Given.PlusAsync(user, maximumGuilds: 2, existing);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "plus add", target);

        response.Description.Should().Contain("Successfully added");
        (await scenario.State.PlusGuildStateAsync(target, user)).Should().Be("enabled");
    }

    [Fact]
    public async Task Remove_DisablesOwnedPlusServer()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.PlusAsync(user, maximumGuilds: 2, guild);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "plus remove", guild);

        response.ShouldBeSuccess();
        (await scenario.State.PlusGuildStateAsync(guild, user)).Should().Be("user_disabled");
    }

    [Fact]
    public async Task Show_NonMemberReceivesMembershipInformation()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "plus show");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("TaylorBot Plus");
    }
}
