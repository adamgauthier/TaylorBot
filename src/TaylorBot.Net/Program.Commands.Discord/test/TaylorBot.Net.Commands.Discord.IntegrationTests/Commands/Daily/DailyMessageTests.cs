using FluentAssertions;
using System.Net;
using System.Diagnostics;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Daily;

[Trait("Command", "daily claim")]
public sealed class DailyMessageTests(DataServices data)
{
    private const string Announcement = "Explore `/help`, protect your points with `/taypoints succession`, and show `/lastfm current`.";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CommandsAvailableAtStartup_RenderAsMentions(bool prefix)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.DailyMessageAsync(Announcement);

        var response = await ClaimAsync(scenario, user, prefix);

        response.ShouldBeSuccess();
        AssertExistingCommandMentions(response, scenario.DiscordApi);
        response.Description.Should().Contain($"</help:{scenario.DiscordApi.GetCommandId("help")}>");
        scenario.DiscordApi.RequestsFor("GET", $"applications/{DiscordApi.ApplicationId}/commands").Should().ContainSingle();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CommandPublishedAfterStartup_RendersAsMention(bool prefix)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api => api.SetGlobalCommandAvailable("help", available: false));
        var user = await scenario.Given.UserAsync();
        await scenario.Given.DailyMessageAsync(Announcement);
        scenario.DiscordApi.SetGlobalCommandAvailable("help", available: true);

        var response = await ClaimAsync(scenario, user, prefix);

        response.ShouldBeSuccess();
        AssertExistingCommandMentions(response, scenario.DiscordApi);
        response.Description.Should().Contain($"</help:{scenario.DiscordApi.GetCommandId("help")}>");
        scenario.DiscordApi.RequestsFor("GET", $"applications/{DiscordApi.ApplicationId}/commands").Should().HaveCount(2);
    }

    [Fact]
    public async Task UnknownCommands_ShareRefreshBudgetAndLogOnEveryMessage()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        await scenario.Given.DailyMessageAsync("Explore `/hlep` or `/hepl`.");

        var first = await ClaimForNewUserAsync(scenario);
        var second = await ClaimForNewUserAsync(scenario);

        first.ShouldBeSuccess();
        second.ShouldBeSuccess();
        first.Description.Should().Contain("/hlep").And.Contain("/hepl").And.NotContain("</");
        second.Description.Should().Contain("/hlep").And.Contain("/hepl").And.NotContain("</");
        scenario.DiscordApi.RequestsFor("GET", $"applications/{DiscordApi.ApplicationId}/commands").Should().HaveCount(2);
        scenario.Logs.ToString().Should().Contain("Could not resolve command hlep in global", Exactly.Twice())
            .And.Contain("Could not resolve command hepl in global", Exactly.Twice());
    }

    [Fact]
    public async Task MissingCommands_RefreshAgainAtOneHourButNotBefore()
    {
        ScenarioTimeProvider time = new();
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api => api.SetGlobalCommandAvailable("help", available: false), timeProvider: time);
        await scenario.Given.DailyMessageAsync(Announcement);
        var missing = await ClaimForNewUserAsync(scenario);
        scenario.DiscordApi.SetGlobalCommandAvailable("help", available: true);

        time.Advance(TimeSpan.FromHours(1) - TimeSpan.FromTicks(1));
        var throttled = await ClaimForNewUserAsync(scenario);
        time.Advance(TimeSpan.FromTicks(1));
        var refreshed = await ClaimForNewUserAsync(scenario);

        missing.Description.Should().Contain("/help").And.NotContain("</help:");
        throttled.Description.Should().Contain("/help").And.NotContain("</help:");
        refreshed.Description.Should().Contain($"</help:{scenario.DiscordApi.GetCommandId("help")}>");
        scenario.DiscordApi.RequestsFor("GET", $"applications/{DiscordApi.ApplicationId}/commands").Should().HaveCount(3);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task FailedRefresh_PreservesKnownMentionsAndWaitsAnHourBeforeRetry(HttpStatusCode status)
    {
        ScenarioTimeProvider time = new();
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api => api.SetGlobalCommandAvailable("help", available: false), timeProvider: time);
        await scenario.Given.DailyMessageAsync(Announcement);
        scenario.DiscordApi.SetGlobalCommandAvailable("help", available: true);
        scenario.DiscordApi.ExpectRequest("GET", $"applications/{DiscordApi.ApplicationId}/commands",
            new { code = 50013, message = "Lookup unavailable", retry_after = 0.01, global = false }, status);

        var failed = await ClaimForNewUserAsync(scenario);
        var throttled = await ClaimForNewUserAsync(scenario);
        time.Advance(TimeSpan.FromHours(1));
        var recovered = await ClaimForNewUserAsync(scenario);

        failed.ShouldBeSuccess();
        throttled.ShouldBeSuccess();
        recovered.ShouldBeSuccess();
        AssertExistingCommandMentions(failed, scenario.DiscordApi);
        failed.Description.Should().Contain("/help").And.NotContain("</help:");
        throttled.Description.Should().NotContain("</help:");
        recovered.Description.Should().Contain($"</help:{scenario.DiscordApi.GetCommandId("help")}>");
        scenario.DiscordApi.RequestsFor("GET", $"applications/{DiscordApi.ApplicationId}/commands").Should().HaveCount(3);
        scenario.Logs.ToString().Should().Contain("Could not refresh global slash-command mentions");
    }

    [Fact]
    public async Task RateLimitedRefresh_FallsBackWithoutRetryingDiscord()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api => api.SetGlobalCommandAvailable("help", available: false));
        await scenario.Given.DailyMessageAsync(Announcement);
        scenario.DiscordApi.SetGlobalCommandAvailable("help", available: true);
        scenario.DiscordApi.ExpectRequest("GET", $"applications/{DiscordApi.ApplicationId}/commands",
            new { message = "Rate limited", retry_after = 60, global = false }, HttpStatusCode.TooManyRequests);

        var failed = await ClaimForNewUserAsync(scenario);
        var throttled = await ClaimForNewUserAsync(scenario);

        failed.ShouldBeSuccess();
        throttled.ShouldBeSuccess();
        AssertExistingCommandMentions(failed, scenario.DiscordApi);
        failed.Description.Should().NotContain("</help:");
        throttled.Description.Should().NotContain("</help:");
        scenario.DiscordApi.RequestsFor("GET", $"applications/{DiscordApi.ApplicationId}/commands").Should().HaveCount(2);
        scenario.Logs.ToString().Should().Contain("Could not refresh global slash-command mentions");
    }

    [Fact]
    public async Task SimultaneousMissingNames_AwaitOneSharedRefresh()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api =>
            {
                api.SetGlobalCommandAvailable("help", available: false);
                api.SetGlobalCommandAvailable("lastfm", available: false);
            });
        await scenario.Given.DailyMessageAsync(Announcement);
        scenario.DiscordApi.SetGlobalCommandAvailable("help", available: true);
        scenario.DiscordApi.SetGlobalCommandAvailable("lastfm", available: true);
        using var gate = scenario.DiscordApi.PauseCommandLookups();

        var claim = ClaimForNewUserAsync(scenario);
        await gate.WaitForRequestAsync(TestContext.Current.CancellationToken);
        var awaitingRefresh = !claim.IsCompleted;
        gate.Dispose();
        var response = await claim;

        awaitingRefresh.Should().BeTrue();
        response.ShouldBeSuccess();
        response.Description.Should().Contain($"</help:{scenario.DiscordApi.GetCommandId("help")}>");
        AssertExistingCommandMentions(response, scenario.DiscordApi);
        gate.Requests.Should().Be(1);
    }

    [Fact]
    public async Task SlowRefresh_CancelsAndFallsBackWithoutDelayingLaterClaims()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api => api.SetGlobalCommandAvailable("help", available: false));
        await scenario.Given.DailyMessageAsync(Announcement);
        scenario.DiscordApi.SetGlobalCommandAvailable("help", available: true);
        using var gate = scenario.DiscordApi.PauseCommandLookups();
        var elapsed = Stopwatch.StartNew();

        var response = await ClaimForNewUserAsync(scenario);
        elapsed.Stop();
        var later = await ClaimForNewUserAsync(scenario);

        response.ShouldBeSuccess();
        later.ShouldBeSuccess();
        AssertExistingCommandMentions(response, scenario.DiscordApi);
        response.Description.Should().NotContain("</help:");
        later.Description.Should().NotContain("</help:");
        elapsed.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
        gate.Requests.Should().Be(1);
        gate.Cancellations.Should().Be(1);
        scenario.Logs.ToString().Should().Contain("Could not refresh global slash-command mentions");
    }

    private static async Task<DiscordExchange> ClaimForNewUserAsync(CommandsDiscordScenario scenario) =>
        await ClaimAsync(scenario, await scenario.Given.UserAsync(), prefix: false);

    [Fact]
    public async Task FailedStartupLookup_DoesNotRetryUntilOneHour()
    {
        ScenarioTimeProvider time = new();
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            configureDiscord: api => api.ExpectRequest("GET", $"applications/{DiscordApi.ApplicationId}/commands",
                new { code = 50013, message = "Lookup unavailable" }, HttpStatusCode.Forbidden), timeProvider: time);
        await scenario.Given.DailyMessageAsync(Announcement);

        var unavailable = await ClaimForNewUserAsync(scenario);
        time.Advance(TimeSpan.FromHours(1));
        var recovered = await ClaimForNewUserAsync(scenario);

        unavailable.ShouldBeSuccess();
        unavailable.Description.Should().Contain("/help").And.NotContain("</help:");
        recovered.Description.Should().Contain($"</help:{scenario.DiscordApi.GetCommandId("help")}>");
        scenario.DiscordApi.RequestsFor("GET", $"applications/{DiscordApi.ApplicationId}/commands").Should().HaveCount(2);
    }

    [Fact]
    public async Task KnownCommands_RefreshAfterNormalCacheLifetime()
    {
        ScenarioTimeProvider time = new();
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken, timeProvider: time);
        time.Advance(TimeSpan.FromDays(1));
        await scenario.Given.DailyMessageAsync(Announcement);

        var response = await ClaimForNewUserAsync(scenario);

        response.ShouldBeSuccess();
        response.Description.Should().Contain($"</help:{scenario.DiscordApi.GetCommandId("help")}>");
        scenario.DiscordApi.RequestsFor("GET", $"applications/{DiscordApi.ApplicationId}/commands").Should().HaveCount(2);
    }

    private static async Task<DiscordExchange> ClaimAsync(CommandsDiscordScenario scenario, ScenarioUser user, bool prefix)
    {
        var guild = await scenario.Given.GuildAsync(user);
        return prefix
            ? await scenario.Discord.SendMessageAsync(user, guild, "!daily")
            : await scenario.Discord.InvokeSlashCommandAsync(user, "daily claim", guild: guild);
    }

    private static void AssertExistingCommandMentions(DiscordExchange response, DiscordApi api)
    {
        response.Description.Should().Contain($"</taypoints succession:{api.GetCommandId("taypoints")}>")
            .And.Contain($"</lastfm current:{api.GetCommandId("lastfm")}>");
    }
}
