using FluentAssertions;
using System.Net;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Tasks;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Framework;

public sealed class PrefixCommandErrorTests(DataServices data)
{
    [Fact]
    public async Task TypeReaderException_LogsFailureAndSendsGenericError()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var target = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        scenario.DiscordApi.ExpectRequest("GET", $"guilds/{guild.Id}/members/{target.Id}",
            new { code = 50013, message = "Synthetic member lookup failure" }, HttpStatusCode.Forbidden);
        scenario.Logs.ExpectError<CommandExecutedHandler>("Unhandled exception in command:");

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!points {target.Id}");

        response.ShouldBeError();
        response.Description.Should().Contain("unknown command error").And.NotContain("Synthetic member lookup failure");
        scenario.DiscordApi.RequestsFor("POST", $"channels/{guild.ChannelId}/messages").Should().ContainSingle();
        scenario.Logs.ToString().Should().Contain("CustomUserTypeReader").And.Contain("HttpException");
    }

    [Fact]
    public async Task InvalidArgument_ReturnsUsageInsteadOfGenericError()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!points unfindable-user");

        response.ShouldBeError();
        response.Description.Should().Contain("Format:").And.Contain("Could not find user").And.NotContain("unknown command error");
    }

    [Theory]
    [InlineData("!points")]
    [InlineData("!points unfindable-user")]
    public async Task ReplyFailure_DoesNotRetryAndAllowsSubsequentCommands(string message)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 13);
        var guild = await scenario.Given.GuildAsync(user);
        scenario.DiscordApi.ExpectRequest("POST", $"channels/{guild.ChannelId}/messages",
            new { code = 50013, message = "Synthetic reply failure" }, HttpStatusCode.Forbidden);
        scenario.Logs.ExpectError<BackgroundTasks>("Background task CommandHandler failed");

        var failed = await scenario.Discord.SendMessageAsync(user, guild, message);
        var subsequent = await scenario.Discord.SendMessageAsync(user, guild, "!points");

        failed.Requests.Should().ContainSingle();
        subsequent.ShouldBeSuccess();
        subsequent.Description.Should().Contain("13");
        scenario.Logs.ToString().Should().Contain("Synthetic reply failure");
    }

    [Fact]
    public async Task TypeReaderException_WhenErrorReplyFails_DoesNotRetry()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var target = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        scenario.DiscordApi.ExpectRequest("GET", $"guilds/{guild.Id}/members/{target.Id}",
            new { code = 50013, message = "Synthetic member lookup failure" }, HttpStatusCode.Forbidden);
        scenario.DiscordApi.ExpectRequest("POST", $"channels/{guild.ChannelId}/messages",
            new { code = 50013, message = "Synthetic reply failure" }, HttpStatusCode.Forbidden);
        scenario.Logs.ExpectError<CommandExecutedHandler>("Unhandled exception in command:");
        scenario.Logs.ExpectError<BackgroundTasks>("Background task CommandHandler failed");

        var failed = await scenario.Discord.SendMessageAsync(user, guild, $"!points {target.Id}");

        failed.Requests.Should().ContainSingle();
        failed.Description.Should().Contain("unknown command error");
    }
}
