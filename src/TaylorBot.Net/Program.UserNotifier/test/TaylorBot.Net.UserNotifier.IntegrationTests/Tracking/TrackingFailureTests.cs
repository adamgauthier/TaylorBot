using TaylorBot.Net.MessagesTracker.Domain;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Tracking;

public sealed class TrackingFailureTests(DataServices data)
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task OpposingMemberBatches_SerializeWithoutDeadlockOrLostTracking(bool includeMinutes, bool pointCycle)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        if (pointCycle)
        {
            await scenario.RunCyclesAsync(UserNotifierJob.Minutes, count: 4);
        }
        var first = await scenario.Given.UserAsync();
        var second = await scenario.Given.UserAsync(username: "Second");
        var firstGuild = await scenario.Given.GuildAsync(first);
        var secondGuild = await scenario.Given.GuildAsync(second);
        if (includeMinutes)
        {
            await scenario.Given.ActiveMemberAsync(firstGuild, first);
            await scenario.Given.ActiveMemberAsync(secondGuild, second);
        }
        var firstMessage = await scenario.Discord.MessageAsync(firstGuild, first, "First message");
        var secondMessage = await scenario.Discord.MessageAsync(secondGuild, second, "Second message");
        await using var overlap = await scenario.Given.OpposingTrackingBatchesAsync(firstMessage, secondMessage, pauseMinutes: includeMinutes);

        var flush = scenario.RunJobAsync(UserNotifierJob.LastSpoke);
        await overlap.ReleaseWhenBatchesContendAsync();
        await flush;

        scenario.Logs.ToString().Should().NotContain("40P01").And.NotContain("deadlock detected");

        var firstMember = await scenario.State.MemberAsync(firstGuild, first);
        var secondMember = await scenario.State.MemberAsync(secondGuild, second);
        firstMember.Messages.Should().Be(1);
        firstMember.Words.Should().Be(2);
        firstMember.LastSpokeAt.Should().BeCloseTo(firstMessage.Timestamp.UtcDateTime, TimeSpan.FromMilliseconds(1));
        secondMember.Messages.Should().Be(1);
        secondMember.Words.Should().Be(2);
        secondMember.LastSpokeAt.Should().BeCloseTo(secondMessage.Timestamp.UtcDateTime, TimeSpan.FromMilliseconds(1));
        if (includeMinutes)
        {
            firstMember.Minutes.Should().Be(1);
            secondMember.Minutes.Should().Be(1);
            firstMember.Experience.Should().Be(pointCycle ? 1 : 0);
            secondMember.Experience.Should().Be(pointCycle ? 1 : 0);
            (await scenario.State.TaypointsAsync(first)).Should().Be(pointCycle ? 1 : 0);
            (await scenario.State.TaypointsAsync(second)).Should().Be(pointCycle ? 1 : 0);
        }
    }

    [Theory]
    [InlineData(UserNotifierJob.LastSpoke, "PersistQueuedLastSpokeUpdatesAsync")]
    [InlineData(UserNotifierJob.ChannelMessages, "PersistQueuedMessageCountIncrementsAsync")]
    [InlineData(UserNotifierJob.MemberMessages, "PersistQueuedMessagesAndWordsAsync")]
    public async Task FailedWrite_PreservesQueuedTrackingForNextCycle(UserNotifierJob job, string operation)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await using var failure = await scenario.Given.RejectTrackingWritesAsync(job);
        await scenario.Discord.MessageAsync(guild, user, "Recover this message");
        scenario.Logs.ExpectError<MessagesTrackerDomainService>($"Unhandled exception in {operation}.");

        await scenario.RunJobAsync(job);
        await failure.DisposeAsync();
        var latest = await scenario.Discord.MessageAsync(guild, user, "And newer");
        await scenario.RunJobAsync(job);

        var member = await scenario.State.MemberAsync(guild, user);
        member.Messages.Should().Be(2);
        member.Words.Should().Be(5);
        member.LastSpokeAt.Should().BeCloseTo(latest.Timestamp.UtcDateTime, TimeSpan.FromMilliseconds(1));
        (await scenario.State.ChannelMessagesAsync(guild)).Should().Be(2);
    }

    [Fact]
    public async Task LegacyPendingLastSpoke_RecoversAlongsideNewUserFields()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var first = await scenario.Discord.MessageAsync(guild, user, "Previous message");
        await scenario.Given.QueueLegacyLastSpokeAsync(first with { Timestamp = first.Timestamp.AddDays(-1) });
        await using var rejection = await scenario.Given.RejectTrackingWritesAsync(UserNotifierJob.LastSpoke);
        scenario.Logs.ExpectError<MessagesTrackerDomainService>("Unhandled exception in PersistQueuedLastSpokeUpdatesAsync.");
        await scenario.RunJobAsync(UserNotifierJob.LastSpoke);
        await rejection.DisposeAsync();

        var latest = await scenario.Discord.MessageAsync(guild, user, "Current message");

        (await scenario.State.LastSpokeQueueFieldsAsync(pending: true)).Should().Equal($"guild:{guild.Id}:channel:{user.Id}");
        (await scenario.State.LastSpokeQueueFieldsAsync()).Should().Equal($"guild:{guild.Id}:user:{user.Id}");
        await scenario.RunJobAsync(UserNotifierJob.LastSpoke);

        (await scenario.State.MemberAsync(guild, user)).LastSpokeAt.Should()
            .BeCloseTo(latest.Timestamp.UtcDateTime, TimeSpan.FromMilliseconds(1));
        (await scenario.State.LastSpokeQueueFieldsAsync()).Should().BeEmpty();
        (await scenario.State.LastSpokeQueueFieldsAsync(pending: true)).Should().BeEmpty();
    }

    [Fact]
    public async Task RejectedMember_RollsBackChunkBeforeRetrying()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var first = await scenario.Given.UserAsync();
        var second = await scenario.Given.UserAsync(username: "Second");
        var firstGuild = await scenario.Given.GuildAsync(first);
        var secondGuild = await scenario.Given.GuildAsync(second);
        await scenario.Discord.MessageAsync(firstGuild, first, "First message");
        await scenario.Discord.MessageAsync(secondGuild, second, "Second message");
        await using var failure = await scenario.Given.RejectTrackingWritesAsync(UserNotifierJob.MemberMessages, onlyUser: second);
        scenario.Logs.ExpectError<MessagesTrackerDomainService>("Unhandled exception in PersistQueuedMessagesAndWordsAsync.");

        await scenario.RunJobAsync(UserNotifierJob.MemberMessages);
        (await scenario.State.MemberAsync(firstGuild, first)).Messages.Should().Be(0);
        (await scenario.State.MemberAsync(secondGuild, second)).Messages.Should().Be(0);
        await failure.DisposeAsync();
        await scenario.RunJobAsync(UserNotifierJob.MemberMessages);

        (await scenario.State.MemberAsync(firstGuild, first)).Messages.Should().Be(1);
        (await scenario.State.MemberAsync(secondGuild, second)).Messages.Should().Be(1);
    }

    [Theory]
    [InlineData(UserNotifierJob.LastSpoke, "PersistQueuedLastSpokeUpdatesAsync")]
    [InlineData(UserNotifierJob.MemberMessages, "PersistQueuedMessagesAndWordsAsync")]
    [InlineData(UserNotifierJob.ChannelMessages, "PersistQueuedMessageCountIncrementsAsync")]
    public async Task RejectedLaterChunk_RetriesOnlyUncommittedTracking(UserNotifierJob job, string operation)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var message = await scenario.Discord.MessageAsync(guild, user, "Backlogged message");
        var backlog = await scenario.Given.ExpandTrackingBacklogAsync(message, count: 1001);
        await using var failure = await scenario.Given.RejectLaterTrackingChunkAsync(job);
        scenario.Logs.ExpectError<MessagesTrackerDomainService>($"Unhandled exception in {operation}.");

        await scenario.RunJobAsync(job);

        var rejected = await scenario.State.TrackingBacklogProgressAsync(job, backlog);
        rejected.Persisted.Should().Be(500);
        rejected.Queued.Should().Be(501);
        rejected.Invalid.Should().Be(0);

        await failure.DisposeAsync();
        await scenario.RunJobAsync(job);

        var recovered = await scenario.State.TrackingBacklogProgressAsync(job, backlog);
        recovered.Persisted.Should().Be(1001);
        recovered.Queued.Should().Be(0);
        recovered.Invalid.Should().Be(0);
    }

    [Theory]
    [InlineData(UserNotifierJob.LastSpoke, "PersistQueuedLastSpokeUpdatesAsync")]
    [InlineData(UserNotifierJob.MemberMessages, "PersistQueuedMessagesAndWordsAsync")]
    [InlineData(UserNotifierJob.ChannelMessages, "PersistQueuedMessageCountIncrementsAsync")]
    public async Task FailedAcknowledgement_ReplaysCommittedChunkAndFinishesBacklog(UserNotifierJob job, string operation)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var message = await scenario.Discord.MessageAsync(guild, user, "Backlogged message");
        var backlog = await scenario.Given.ExpandTrackingBacklogAsync(message, count: 1001);
        await using var failure = await scenario.Given.RejectTrackingAcknowledgementsAsync();
        foreach (var name in new[] { "PersistQueuedLastSpokeUpdatesAsync", "PersistQueuedMessagesAndWordsAsync", "PersistQueuedMessageCountIncrementsAsync" })
        {
            scenario.Logs.ExpectError<MessagesTrackerDomainService>($"Unhandled exception in {name}.");
        }

        await scenario.RunJobAsync(job);

        var interrupted = await scenario.State.TrackingBacklogProgressAsync(job, backlog);
        interrupted.Persisted.Should().Be(500);
        interrupted.Invalid.Should().Be(0);
        var delayedReplay = await scenario.Given.RememberPendingTrackingAsync(job);

        await failure.DisposeAsync();
        await scenario.Given.FlushTrackingFromNewWorkerAsync(job);
        await delayedReplay.ReplayAsync();
        await scenario.RunJobAsync(job);

        var recovered = await scenario.State.TrackingBacklogProgressAsync(job, backlog);
        recovered.Persisted.Should().Be(1001, operation);
        recovered.Queued.Should().Be(0);
        recovered.Invalid.Should().Be(0);
    }

    [Theory]
    [InlineData(UserNotifierJob.LastSpoke)]
    [InlineData(UserNotifierJob.MemberMessages)]
    [InlineData(UserNotifierJob.ChannelMessages)]
    public async Task ConcurrentWorkers_SerializePublicationAndApplyEachChunkOnce(UserNotifierJob job)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var message = await scenario.Discord.MessageAsync(guild, user, "Backlogged message");
        var backlog = await scenario.Given.ExpandTrackingBacklogAsync(message, count: 1001);
        await using var overlap = await scenario.Given.PauseTrackingQueueAsync(job);

        var scheduled = scenario.RunJobAsync(job);
        var secondWorker = scenario.Given.FlushTrackingFromNewWorkerAsync(job);
        await overlap.ReleaseWhenWorkersWaitAsync();
        await Task.WhenAll(scheduled, secondWorker);
        await scenario.RunJobAsync(job);

        var progress = await scenario.State.TrackingBacklogProgressAsync(job, backlog);
        progress.Persisted.Should().Be(1001);
        progress.Queued.Should().Be(0);
        progress.Invalid.Should().Be(0);
    }

    [Theory]
    [InlineData(UserNotifierJob.LastSpoke)]
    [InlineData(UserNotifierJob.MemberMessages)]
    [InlineData(UserNotifierJob.ChannelMessages)]
    public async Task LostCommitResponse_RetriesFromDurableCheckpointWithoutDoubleCounting(UserNotifierJob job)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var message = await scenario.Discord.MessageAsync(guild, user, "Backlogged message");
        var backlog = await scenario.Given.ExpandTrackingBacklogAsync(message, count: 1001);

        await scenario.Given.FlushTrackingWithLostCommitResponseAsync(job);

        var interrupted = await scenario.State.TrackingBacklogProgressAsync(job, backlog);
        interrupted.Persisted.Should().Be(500);
        interrupted.Queued.Should().Be(1001);
        interrupted.Invalid.Should().Be(0);
        await scenario.Given.FlushTrackingFromNewWorkerAsync(job);
        await scenario.RunJobAsync(job);

        var recovered = await scenario.State.TrackingBacklogProgressAsync(job, backlog);
        recovered.Persisted.Should().Be(1001);
        recovered.Queued.Should().Be(0);
        recovered.Invalid.Should().Be(0);
    }

    [Theory]
    [InlineData(UserNotifierJob.LastSpoke, "PersistQueuedLastSpokeUpdatesAsync")]
    [InlineData(UserNotifierJob.MemberMessages, "PersistQueuedMessagesAndWordsAsync")]
    [InlineData(UserNotifierJob.ChannelMessages, "PersistQueuedMessageCountIncrementsAsync")]
    public async Task SequenceGap_RejectsLaterBatchWithoutDiscardingItsPayload(UserNotifierJob job, string operation)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Discord.MessageAsync(guild, user, "Preserve this message");
        await using var rejection = await scenario.Given.RejectTrackingWritesAsync(job);
        scenario.Logs.ExpectError<MessagesTrackerDomainService>($"Unhandled exception in {operation}.");
        await scenario.RunJobAsync(job);
        await rejection.DisposeAsync();
        await using var gap = await scenario.Given.SkipPendingTrackingSequenceAsync(job);

        var retry = () => scenario.Given.FlushTrackingFromNewWorkerAsync(job);
        await retry.Should().ThrowAsync<InvalidOperationException>().WithMessage("*sequence gap*");
        await scenario.Given.RememberPendingTrackingAsync(job);

        await gap.DisposeAsync();
        await scenario.RunJobAsync(job);
        var member = await scenario.State.MemberAsync(guild, user);
        member.Messages.Should().Be(1);
        member.Words.Should().Be(3);
        member.LastSpokeAt.Should().NotBeNull();
        (await scenario.State.ChannelMessagesAsync(guild)).Should().Be(1);
    }
}
