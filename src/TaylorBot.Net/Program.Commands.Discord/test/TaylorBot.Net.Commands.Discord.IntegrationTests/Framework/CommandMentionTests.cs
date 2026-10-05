using FluentAssertions;
using System.Net;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Framework;

public sealed class CommandMentionTests(DataServices data)
{
    [Fact]
    public async Task PreAcknowledgementError_DoesNotWaitForMissingMention()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api => api.SetGlobalCommandAvailable("command", available: false));
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.KnownCommandAsync("modmail message-mods");
        await scenario.Given.DisabledServerCommandAsync(guild, "modmail message-mods");
        scenario.DiscordApi.SetGlobalCommandAvailable("command", available: true);
        using var gate = scenario.DiscordApi.PauseCommandLookups();

        var pendingResponse = scenario.Discord.InvokeSlashCommandAsync(user, "modmail message-mods", guild);
        await gate.WaitForMessageAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        gate.Dispose();
        var response = await pendingResponse;
        var subsequent = await scenario.Discord.InvokeSlashCommandAsync(user, "modmail message-mods", guild);

        response.ShouldBeError();
        subsequent.ShouldBeError();
        response.Description.Should().Contain("/command server-enable").And.NotContain("</command server-enable:");
        subsequent.Description.Should().Contain($"</command server-enable:{scenario.DiscordApi.GetCommandId("command")}>");
        gate.Requests.Should().Be(1);
        gate.Cancellations.Should().Be(0);
    }

    [Fact]
    public async Task InteractionId_DoesNotWaitForOrRequestLookup()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api => api.SetGlobalCommandAvailable("modmail", available: false));
        var user = await scenario.Given.UserAsync();
        scenario.DiscordApi.SetGlobalCommandAvailable("modmail", available: true);
        using var gate = scenario.DiscordApi.PauseCommandLookups();

        var pendingResponse = scenario.Discord.InvokeSlashCommandAsync(user, "modmail message-mods");
        await gate.WaitForMessageAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        gate.Dispose();
        var response = await pendingResponse;

        response.ShouldBeError();
        response.Description.Should().Contain($"</modmail message-mods:{scenario.DiscordApi.GetCommandId("modmail")}>");
        gate.Requests.Should().Be(0);
    }

    [Fact]
    public async Task StaleCachedMention_SendsResponseBeforeRefreshCompletes()
    {
        ScenarioTimeProvider time = new();
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken, timeProvider: time);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        time.Advance(TimeSpan.FromDays(1));
        using var gate = scenario.DiscordApi.PauseCommandLookups();

        var pendingResponse = scenario.Discord.SendMessageAsync(user, guild, "!avatar");
        await gate.WaitForMessageAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        gate.Dispose();
        var response = await pendingResponse;

        response.ShouldBeSuccess();
        response.Description.Should().Contain($"</avatar:{scenario.DiscordApi.GetCommandId("avatar")}>");
        gate.Requests.Should().Be(1);
        gate.Cancellations.Should().Be(0);
    }

    [Theory]
    [InlineData("gender clear", "server", "server population")]
    [InlineData("birthday role", "plus", "plus show")]
    public async Task SlashResponse_AwaitsNewlyPublishedMention(string command, string publishedCommand, string mentionedRoute)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api => api.SetGlobalCommandAvailable(publishedCommand, available: false));
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        scenario.DiscordApi.SetGlobalCommandAvailable(publishedCommand, available: true);
        using var gate = scenario.DiscordApi.PauseCommandLookups();

        var pendingResponse = scenario.Discord.InvokeSlashCommandAsync(user, command, guild);
        await gate.WaitForRequestAsync(TestContext.Current.CancellationToken);
        gate.Dispose();
        var response = await pendingResponse;

        response.Description.Should().Contain($"</{mentionedRoute}:{scenario.DiscordApi.GetCommandId(publishedCommand)}>");
        gate.Requests.Should().Be(1);
    }

    [Theory]
    [InlineData("!choose a, b", "choose", "choose")]
    [InlineData("!setbirthday", "birthday", "birthday set")]
    [InlineData("!profile", "birthday", "birthday age")]
    [InlineData("!avatar", "avatar", "avatar")]
    [InlineData("!setage", "birthday", "birthday set")]
    public async Task PrefixResponse_AwaitsNewlyPublishedMention(string message, string publishedCommand, string mentionedRoute)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api => api.SetGlobalCommandAvailable(publishedCommand, available: false));
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        scenario.DiscordApi.SetGlobalCommandAvailable(publishedCommand, available: true);
        using var gate = scenario.DiscordApi.PauseCommandLookups();

        var pendingResponse = scenario.Discord.SendMessageAsync(user, guild, message);
        await gate.WaitForRequestAsync(TestContext.Current.CancellationToken);
        gate.Dispose();
        var response = await pendingResponse;

        response.Description.Should().Contain($"</{mentionedRoute}:{scenario.DiscordApi.GetCommandId(publishedCommand)}>");
        gate.Requests.Should().Be(1);
    }

    [Fact]
    public async Task FailedRefresh_UsesInteractionIdOnlyForMatchingCommand()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api =>
            {
                api.SetGlobalCommandAvailable("gender", available: false);
                api.SetGlobalCommandAvailable("server", available: false);
            });
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        scenario.DiscordApi.SetGlobalCommandAvailable("gender", available: true);
        scenario.DiscordApi.SetGlobalCommandAvailable("server", available: true);
        scenario.DiscordApi.ExpectRequest("GET", $"applications/{DiscordApi.ApplicationId}/commands",
            new { code = 50013, message = "Lookup unavailable" }, HttpStatusCode.Forbidden);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "gender clear", guild);

        response.ShouldBeSuccess();
        response.Description.Should().Contain($"</gender set:{scenario.DiscordApi.GetCommandId("gender")}>")
            .And.Contain("/server population").And.NotContain("</server population:");
        scenario.DiscordApi.RequestsFor("GET", $"applications/{DiscordApi.ApplicationId}/commands").Should().HaveCount(2);
        scenario.Logs.ToString().Should().Contain("Could not refresh global slash-command mentions");
    }
}
