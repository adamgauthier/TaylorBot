using TaylorBot.Net.PatreonSync.Domain;
using TaylorBot.Net.UserNotifier.IntegrationTests.Discord;
using TaylorBot.Net.UserNotifier.IntegrationTests.ExternalApis;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Notifications;

public sealed class PatreonTests(DataServices data)
{
    private Task<UserNotifierScenario> CreateAsync() => UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
        job: UserNotifierJob.Patreon, settings: new Dictionary<string, string?> { ["PatreonSync:Enabled"] = "true" });

    [Fact]
    public async Task DisabledSync_DoesNotRequestPatreon()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.Patreon);

        await scenario.RunJobAsync(UserNotifierJob.Patreon);

        scenario.External.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task PaginatedMembers_SyncsDiscordAccountsAndWelcomesActivePatrons()
    {
        await using var scenario = await CreateAsync();
        var active = await scenario.Given.UserAsync();
        var inactive = await scenario.Given.UserAsync(username: "Inactive");
        var command = scenario.DiscordApi.GlobalCommand("plus");
        scenario.Patreon.Members([new(active), new(inactive, Active: false), new(null)], paginated: true);
        scenario.DiscordApi.ExpectMessage(NotifierDiscordApi.DmChannel(active.Id));

        var output = await scenario.RunJobAsync(UserNotifierJob.Patreon);

        output.Text.Should().Contain("Welcome to TaylorBot Plus").And.Contain($"</plus show:{command}>");
        (await scenario.State.PatronAsync(active))!.Active.Should().BeTrue();
        (await scenario.State.PatronAsync(inactive))!.Active.Should().BeFalse();
        (await scenario.State.PatronCountAsync()).Should().Be(2);
        scenario.External.Requests.Should().HaveCount(2).And.OnlyContain(request => request.Headers["Authorization"] == "Bearer synthetic");
    }

    [Fact]
    public async Task Welcome_UnavailableCommandFallsBackToPlainText()
    {
        await using var scenario = await CreateAsync();
        var user = await scenario.Given.UserAsync();
        scenario.DiscordApi.Resource($"applications/{NotifierDiscordApi.BotId}/commands", Array.Empty<object>());
        scenario.Patreon.Members([new(user)]);
        scenario.DiscordApi.ExpectMessage(NotifierDiscordApi.DmChannel(user.Id));

        var output = await scenario.RunJobAsync(UserNotifierJob.Patreon);

        output.Text.Should().Contain("Welcome to TaylorBot Plus").And.Contain("/plus show").And.NotContain("</plus show:");
        scenario.Logs.ToString().Should().Contain("Could not resolve command plus in global");
        (await scenario.State.PatronAsync(user))!.Active.Should().BeTrue();
    }

    [Fact]
    public async Task PaidCharge_RewardsAndNotifiesOnlyOnce()
    {
        await using var scenario = await CreateAsync();
        var user = await scenario.Given.UserAsync(taypoints: 100);
        await scenario.Given.PatronAsync(user);
        scenario.Patreon.Members([new(user, Paid: true)]);
        scenario.DiscordApi.ExpectMessage(NotifierDiscordApi.DmChannel(user.Id));

        var output = await scenario.RunJobAsync(UserNotifierJob.Patreon);
        var repeated = await scenario.RunJobAsync(UserNotifierJob.Patreon);

        output.Text.Should().Contain("2,000").And.Contain("2,100");
        repeated.Messages.Should().BeEmpty();
        (await scenario.State.TaypointsAsync(user)).Should().Be(2100);
        (await scenario.State.PatronAsync(user))!.RewardedCharge.Should().Be(PatreonFixtures.ChargeDate);
    }

    [Theory]
    [InlineData(false, 200)]
    [InlineData(true, 0)]
    public async Task LostEntitlement_DisablesGuildAndNotifiesPatron(bool active, int entitledCents)
    {
        await using var scenario = await CreateAsync();
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user, name: "Affected guild");
        await scenario.Given.PatronAsync(user, guild);
        scenario.Patreon.Members([new(user, active, entitledCents)]);
        scenario.DiscordApi.ExpectMessage(NotifierDiscordApi.DmChannel(user.Id));

        var output = await scenario.RunJobAsync(UserNotifierJob.Patreon);

        output.Text.Should().Contain("Affected guild");
        (await scenario.State.PlusGuildStateAsync(guild)).Should().Be("auto_disabled");
    }

    [Fact]
    public async Task RejectedWelcome_DoesNotDiscardPatron()
    {
        await using var scenario = await CreateAsync();
        var user = await scenario.Given.UserAsync();
        scenario.DiscordApi.GlobalCommand("plus");
        scenario.Patreon.Members([new(user)]);
        scenario.DiscordApi.RejectMessage(NotifierDiscordApi.DmChannel(user.Id));

        await scenario.RunJobAsync(UserNotifierJob.Patreon);

        (await scenario.State.PatronAsync(user))!.Active.Should().BeTrue();
    }

    [Fact]
    public async Task MalformedResponse_ReportsFailureWithoutAddingPatrons()
    {
        await using var scenario = await CreateAsync();
        scenario.External.Raw("GET", PatreonFixtures.MembersUri, "{invalid", "application/json");
        scenario.Logs.ExpectError<PatreonSyncDomainService>("Unhandled exception in SyncPatreonSupportersAsync");

        await scenario.RunJobAsync(UserNotifierJob.Patreon);

        (await scenario.State.PatronCountAsync()).Should().Be(0);
    }
}
