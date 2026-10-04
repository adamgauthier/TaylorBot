using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Help;

[Trait("Command", "help")]
public sealed class HelpTests(DataServices data)
{
    [Fact]
    public async Task Help_SelectsCategoryAndReturnsHome()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var home = await scenario.Discord.InvokeSlashCommandAsync(user, "help");
        home.ShouldBeSuccess();
        home.Description.Should().Contain("TaylorBot").And.Contain("taylorbot.app").And.Contain("Pick a command category");
        var option = home.Message.GetProperty("components")[0].GetProperty("components")[0].GetProperty("options")[1];
        var category = await scenario.Discord.SelectAsync(user, home, option.GetProperty("value").GetString()!);
        category.ShouldBeSuccess();
        category.Description.Should().Contain(option.GetProperty("label").GetString()!).And.Contain("</");
        var restored = await scenario.Discord.SelectAsync(user, category, "home");

        restored.Description.Should().Be(home.Description);
    }

    [Fact]
    public async Task Category_AnotherUserCannotChangeOriginalHelp()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync();
        var home = await scenario.Discord.InvokeSlashCommandAsync(user, "help");

        var response = await scenario.Discord.SelectAsync(other, home, "home");

        response.Requests.Should().ContainSingle().Which.Body!.Value.GetProperty("type").GetInt32().Should().Be(6);
    }

    [Fact]
    public async Task LegacyHelp_PointsToSlashHelpWithoutInteractiveMenu()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!help ignored command");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("TaylorBot").And.Contain("Use </help:").And.NotContain("Pick a command category");
        response.Message.TryGetProperty("components", out _).Should().BeFalse();
    }
}
