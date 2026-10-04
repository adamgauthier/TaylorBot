using TaylorBot.Net.MessagesTracker.Domain;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Tracking;

public sealed class TrackingFailureTests(DataServices data)
{
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
    public async Task RejectedMember_RollsBackWholeBatchBeforeRetrying()
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
}
