using TaylorBot.Net.UserNotifier.IntegrationTests.Discord;
using TaylorBot.Net.UserNotifier.Program.Jobs;
using TaylorBot.Net.UserNotifier.IntegrationTests.ExternalApis;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Lifecycle;

public sealed class ScheduledJobsTests(DataServices data)
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Ready_StartsGlobalJobsOnceAcrossShardsAndRepeatedReady(int shards)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:ShardCount"] = $"{shards}" });
        var before = scenario.Jobs.Jobs;

        await scenario.RepeatReadyAsync();

        scenario.Jobs.Jobs.Should().BeEquivalentTo(before);
        before.Should().HaveCount(Enum.GetValues<UserNotifierJob>().Length).And.OnlyHaveUniqueItems(job => job.Name);
        before.Where(job => job.Name is UserNotifierJob.Minutes or UserNotifierJob.LastSpoke or UserNotifierJob.ChannelMessages or UserNotifierJob.MemberMessages)
            .Should().OnlyContain(job => job.CompletedCycles == 1);
        before.Where(job => job.Name is not (UserNotifierJob.Minutes or UserNotifierJob.LastSpoke or UserNotifierJob.ChannelMessages or UserNotifierJob.MemberMessages))
            .Should().OnlyContain(job => job.CompletedCycles == 0);
    }

    [Fact]
    public async Task InitialDelay_DefersReminderUntilDue()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.Reminders);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.ReminderAsync(user);
        scenario.DiscordApi.ExpectMessage(NotifierDiscordApi.DmChannel(user.Id));

        var before = await scenario.AdvanceAsync(TimeSpan.FromMilliseconds(999));
        var due = await scenario.AdvanceAsync(TimeSpan.FromMilliseconds(1));

        before.Messages.Should().BeEmpty();
        due.Messages.Should().ContainSingle();
    }

    [Fact]
    public async Task ReloadedInterval_AppliesAfterCurrentCycle()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.Reminders);
        await scenario.ReloadAsync("ReminderNotifier:TimeSpanBetweenReminderChecks", "00:02:00");

        await scenario.RunJobAsync(UserNotifierJob.Reminders);

        scenario.Jobs.Jobs.Single(job => job.Name == UserNotifierJob.Reminders).NextRunAt.Should().Be(scenario.Now + TimeSpan.FromMinutes(2));
    }

    [Fact]
    public async Task Shutdown_InterruptsIdleWaitsAndPreventsFurtherCycles()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var before = scenario.Jobs.Jobs.ToDictionary(job => job.Name, job => job.CompletedCycles);

        await scenario.StopAsync();

        scenario.Jobs.Jobs.Should().OnlyContain(job => !job.IsRunning && job.NextRunAt == null && job.CompletedCycles == before[job.Name]);
    }

    [Fact]
    public async Task Shutdown_DrainsActivePacedNotificationsAndFlushesTracking()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRewards);
        var first = await scenario.Given.UserAsync();
        var second = await scenario.Given.UserAsync(username: "Second");
        var guild = await scenario.Given.GuildAsync(first);
        await scenario.Given.BirthdayAsync(first);
        await scenario.Given.BirthdayAsync(second);
        await scenario.Discord.MessageAsync(guild, first, "Pending message");
        scenario.DiscordApi.ExpectMessage(NotifierDiscordApi.DmChannel(first.Id));
        scenario.DiscordApi.ExpectMessage(NotifierDiscordApi.DmChannel(second.Id));
        await scenario.RunUntilPacingAsync(UserNotifierJob.BirthdayRewards);
        scenario.Jobs.Jobs.Single(job => job.Name == UserNotifierJob.BirthdayRewards).IsRunning.Should().BeTrue();

        await scenario.StopAsync();

        scenario.Jobs.Jobs.Single(job => job.Name == UserNotifierJob.BirthdayRewards).CompletedCycles.Should().Be(1);
        (await scenario.State.MemberAsync(guild, first)).Messages.Should().Be(1);
        (await scenario.State.TaypointsAsync(first)).Should().Be(1989);
        (await scenario.State.TaypointsAsync(second)).Should().Be(1989);
    }

    [Fact]
    public async Task ShutdownDeadline_ReportsUnfinishedWorkWithoutFlushingUntilItCompletes()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRewards);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.BirthdayAsync(user);
        await scenario.Discord.MessageAsync(guild, user, "Pending message");
        scenario.DiscordApi.ExpectMessage(NotifierDiscordApi.DmChannel(user.Id));
        await scenario.RunUntilPacingAsync(UserNotifierJob.BirthdayRewards);
        scenario.Logs.ExpectError<UserNotifierJobs>("Scheduled shutdown did not finish");
        using CancellationTokenSource deadline = new();
        await deadline.CancelAsync();

        var stop = () => scenario.RequestStopAsync(deadline.Token);
        await stop.Should().ThrowAsync<AggregateException>();

        scenario.Jobs.Jobs.Single(job => job.Name == UserNotifierJob.BirthdayRewards).IsRunning.Should().BeTrue();
        (await scenario.State.MemberAsync(guild, user)).Messages.Should().Be(0);
        await scenario.StopAsync();
        (await scenario.State.MemberAsync(guild, user)).Messages.Should().Be(1);
        (await scenario.State.TaypointsAsync(user)).Should().Be(1989);
    }

    [Fact]
    public async Task BlockedHttpRequest_DoesNotOverlapCyclesAndFinishesBeforeShutdown()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.Reddit);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.FeedAsync(guild, UserNotifierJob.Reddit);
        scenario.Feeds.Posts(UserNotifierJob.Reddit);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        using var response = scenario.External.Pause(FeedFixtures.RedditListingUri);
        await scenario.RunUntilRequestAsync(UserNotifierJob.Reddit, response);

        await scenario.AdvanceWhileRequestBlockedAsync(UserNotifierJob.Reddit, TimeSpan.FromMinutes(5));
        var stop = scenario.RequestStopAsync(TestContext.Current.CancellationToken);

        stop.IsCompleted.Should().BeFalse();
        scenario.External.Requests.Count(request => request.Method == "GET").Should().Be(1);
        scenario.Jobs.Jobs.Single(job => job.Name == UserNotifierJob.Reddit).CompletedCycles.Should().Be(0);
        response.Dispose();
        await scenario.StopAsync();
        stop.IsCompletedSuccessfully.Should().BeTrue();
        (await scenario.State.FeedCheckpointAsync(guild, UserNotifierJob.Reddit)).Should().Be("post123");
        scenario.Jobs.Jobs.Single(job => job.Name == UserNotifierJob.Reddit).CompletedCycles.Should().Be(1);
    }
}
