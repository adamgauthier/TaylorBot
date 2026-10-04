using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Tracking;

public sealed class MessageTrackingTests(DataServices data)
{
    [Theory]
    [InlineData("Hey", 1)]
    [InlineData("Hey guys, how are you?", 5)]
    [InlineData("Hey   guys", 2)]
    [InlineData("Hey\nguys", 2)]
    public async Task GuildMessage_PersistsMessagesWordsAndLastSpoke(string content, int words)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var message = await scenario.Discord.MessageAsync(guild, user, content);
        await scenario.RunJobAsync(UserNotifierJob.MemberMessages);

        var member = await scenario.State.MemberAsync(guild, user);
        member.Messages.Should().Be(1);
        member.Words.Should().Be(words);
        member.LastSpokeAt.Should().BeCloseTo(message.Timestamp.UtcDateTime, TimeSpan.FromMilliseconds(1));
        (await scenario.State.ChannelMessagesAsync(guild)).Should().Be(1);
    }

    [Fact]
    public async Task Shutdown_FlushesQueuedTrackingWithoutWaitingForNextTick()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Discord.MessageAsync(guild, user, "Hello there");
        (await scenario.State.QueuedMessagesAsync(guild, user)).Should().Be(1);
        (await scenario.State.MemberAsync(guild, user)).Messages.Should().Be(0);

        await scenario.StopAsync();

        var member = await scenario.State.MemberAsync(guild, user);
        member.Messages.Should().Be(1);
        member.Words.Should().Be(2);
        member.LastSpokeAt.Should().NotBeNull();
        (await scenario.State.ChannelMessagesAsync(guild)).Should().Be(1);
    }

    [Fact]
    public async Task SpamChannel_TracksChannelActivityWithoutMemberMessagesOrWords()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.SpamChannelAsync(guild);

        await scenario.Discord.MessageAsync(guild, user, "Spam should not count");
        await scenario.RunJobAsync(UserNotifierJob.MemberMessages);

        var member = await scenario.State.MemberAsync(guild, user);
        member.Messages.Should().Be(0);
        member.Words.Should().Be(0);
        member.LastSpokeAt.Should().NotBeNull();
        (await scenario.State.ChannelMessagesAsync(guild)).Should().Be(1);
    }

    [Fact]
    public async Task RepeatedFlushes_OnlyPersistNewIncrements()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        await scenario.Discord.MessageAsync(guild, user, "First");
        await scenario.RunJobAsync(UserNotifierJob.MemberMessages);
        await scenario.Discord.MessageAsync(guild, user, "Second message");
        await scenario.RunJobAsync(UserNotifierJob.MemberMessages);
        await scenario.RunJobAsync(UserNotifierJob.MemberMessages);

        var member = await scenario.State.MemberAsync(guild, user);
        member.Messages.Should().Be(2);
        member.Words.Should().Be(3);
        (await scenario.State.ChannelMessagesAsync(guild)).Should().Be(2);
    }

    [Fact]
    public async Task DirectMessage_DoesNotIncrementGuildCounters()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        await scenario.Discord.DirectMessageAsync(user, "A private message");
        await scenario.RunJobAsync(UserNotifierJob.MemberMessages);

        (await scenario.State.MemberAsync(guild, user)).Messages.Should().Be(0);
        (await scenario.State.ChannelMessagesAsync(guild)).Should().Be(0);
    }
}
