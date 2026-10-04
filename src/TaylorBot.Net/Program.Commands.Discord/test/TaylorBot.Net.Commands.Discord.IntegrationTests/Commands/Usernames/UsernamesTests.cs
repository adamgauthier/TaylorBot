using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Usernames;

[Trait("Command", "usernames show")]
[Trait("Command", "usernames visibility")]
public sealed class UsernamesTests(DataServices data)
{
    [Fact]
    public async Task Show_PrivateHistoryDoesNotDiscloseNames()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.UsernameHistoryAsync(user, hidden: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "usernames show");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("private").And.NotContain("Enchanted13");
    }

    [Fact]
    public async Task Show_PublicHistoryContainsPreviousUsername()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.UsernameHistoryAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "usernames show");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Enchanted13");
    }

    [Theory]
    [InlineData("private", true)]
    [InlineData("public", false)]
    public async Task Visibility_PersistsSelectedPrivacy(string setting, bool hidden)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.UsernameHistoryAsync(user, hidden: !hidden);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "usernames visibility",
            arguments: [SlashArgument.Text("setting", setting)]);

        response.ShouldBeSuccess();
        (await scenario.State.UsernameHistoryHiddenAsync(user)).Should().Be(hidden);
    }
}
