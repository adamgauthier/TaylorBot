using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Image;

[Trait("Command", "image")]
public sealed class ImageTests(DataServices data)
{
    [Theory]
    [InlineData("rateLimitExceeded", "daily query limit")]
    [InlineData("accessNotConfigured", "unexpected error")]
    public async Task Search_ServiceFailureExplainsCause(string reason, string message)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.PlusAsync(user);
        scenario.External.ImageSearchError(reason);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "image",
            arguments: [SlashArgument.Text("search", "Taylor Swift")]);

        response.ShouldBeError();
        response.Description.Should().Contain(message);
    }

    [Fact]
    public async Task Search_ShowsActualImageResult()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.PlusAsync(user);
        scenario.External.ImageSearch();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "image",
            arguments: [SlashArgument.Text("search", "Taylor Swift")]);

        response.ShouldBeSuccess();
        response.Embed.GetProperty("image").GetProperty("url").GetString().Should().Be("https://images.example.invalid/taylor.jpg");
        response.Embed.GetProperty("title").GetString().Should().Be("Taylor Swift");
        response.Embed.GetProperty("url").GetString().Should().Be("https://example.invalid/taylor");
    }
}
