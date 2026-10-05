using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.LastFm;

[Trait("Command", "lastfm set")]
[Trait("Command", "lastfm clear")]
public sealed class LastFmAccountTests(DataServices data)
{
    [Fact]
    public async Task Set_PersistsLinkedAccount()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm set",
            arguments: [SlashArgument.Text("username", "taylorswift")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("taylorswift");
        (await scenario.State.LastFmAsync(user)).Should().Be("taylorswift");
    }

    [Fact]
    public async Task Clear_RemovesLinkedAccount()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm clear");

        response.ShouldBeSuccess();
        (await scenario.State.LastFmAsync(user)).Should().BeNull();
    }

    [Fact]
    public async Task LegacySet_ParsesProfileUrlAndPersistsUsername()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!lastfm set https://www.last.fm/user/taylorswift");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("https://www.last.fm/user/taylorswift");
        (await scenario.State.LastFmAsync(user)).Should().Be("taylorswift");
    }

    [Theory]
    [InlineData("!lastfm clear")]
    [InlineData("!fm clear")]
    [InlineData("!np clear")]
    [InlineData("!lastfm clear ignored")]
    public async Task LegacyClear_RedirectsWithoutRemovingLinkedAccount(string message)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LastFmAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, message);

        response.Description.Should().Contain("has been moved").And.Contain("/lastfm clear");
        (await scenario.State.LastFmAsync(user)).Should().Be("taylorswift");
    }
}
