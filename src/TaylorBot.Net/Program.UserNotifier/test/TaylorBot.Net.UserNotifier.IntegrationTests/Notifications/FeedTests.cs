using TaylorBot.Net.UserNotifier.IntegrationTests.ExternalApis;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Notifications;

public sealed class FeedTests(DataServices data)
{
    [Theory]
    [InlineData(UserNotifierJob.Reddit, "post123")]
    [InlineData(UserNotifierJob.Youtube, "video123")]
    [InlineData(UserNotifierJob.Tumblr, "https://tmblr.co/post123")]
    public async Task NewPost_NotifiesAndPersistsCheckpointWithoutRepeating(UserNotifierJob job, string checkpoint)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.FeedAsync(guild, job);
        scenario.Feeds.Posts(job);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.RunJobAsync(job);
        var repeated = await scenario.RunJobAsync(job);

        output.Text.Should().Contain("Fresh notification content");
        repeated.Messages.Should().BeEmpty();
        (await scenario.State.FeedCheckpointAsync(guild, job)).Should().Be(checkpoint);
    }

    [Theory]
    [InlineData(UserNotifierJob.Reddit)]
    [InlineData(UserNotifierJob.Youtube)]
    [InlineData(UserNotifierJob.Tumblr)]
    public async Task FailedDelivery_RetainsCheckpointAndTriesAgainNextCycle(UserNotifierJob job)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.FeedAsync(guild, job);
        scenario.Feeds.Posts(job);
        scenario.DiscordApi.RejectMessage(guild.ChannelId, code: 50013);
        FeedFixtures.ExpectCheckerFailure(scenario.Logs, job);

        await scenario.RunJobAsync(job);
        (await scenario.State.FeedCheckpointAsync(guild, job)).Should().BeNull();
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        var retried = await scenario.RunJobAsync(job);

        retried.Messages.Should().ContainSingle();
        (await scenario.State.FeedCheckpointAsync(guild, job)).Should().NotBeNull();
    }

    [Theory]
    [InlineData(UserNotifierJob.Reddit)]
    [InlineData(UserNotifierJob.Youtube)]
    [InlineData(UserNotifierJob.Tumblr)]
    public async Task EmptyFeed_ReportsCheckerFailureWithoutAdvancingCheckpoint(UserNotifierJob job)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.FeedAsync(guild, job);
        scenario.Feeds.Posts(job, empty: true);
        FeedFixtures.ExpectCheckerFailure(scenario.Logs, job);

        var output = await scenario.RunJobAsync(job);

        output.Messages.Should().BeEmpty();
        (await scenario.State.FeedCheckpointAsync(guild, job)).Should().BeNull();
    }

    [Fact]
    public async Task RedditAuthentication_ReusesTokenUntilItExpires()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.Reddit);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.FeedAsync(guild, UserNotifierJob.Reddit);
        scenario.Feeds.Posts(UserNotifierJob.Reddit);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        await scenario.RunJobAsync(UserNotifierJob.Reddit);
        await scenario.RunJobAsync(UserNotifierJob.Reddit);
        scenario.External.Requests.Count(request => request.Method == "POST").Should().Be(1);
        await scenario.AdvanceAsync(TimeSpan.FromHours(1));

        var requests = scenario.External.Requests;
        requests.Count(request => request.Method == "POST").Should().Be(2);
        requests.Where(request => request.Method == "GET").Should().OnlyContain(request => request.Headers["Authorization"] == "Bearer synthetic-access-token");
        requests.First(request => request.Method == "POST").Body.Should().Contain("grant_type=client_credentials");
    }

    [Theory]
    [InlineData(UserNotifierJob.Reddit)]
    [InlineData(UserNotifierJob.Youtube)]
    [InlineData(UserNotifierJob.Tumblr)]
    public async Task FailedDestination_DoesNotPreventOtherDestinationsReceivingPost(UserNotifierJob job)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job);
        var user = await scenario.Given.UserAsync();
        var unavailable = await scenario.Given.GuildAsync(user);
        var available = await scenario.Given.GuildAsync(user);
        await scenario.Given.FeedAsync(unavailable, job);
        await scenario.Given.FeedAsync(available, job);
        scenario.Feeds.Posts(job);
        scenario.DiscordApi.RejectMessage(unavailable.ChannelId, code: 50013);
        scenario.DiscordApi.ExpectMessage(available.ChannelId);
        FeedFixtures.ExpectCheckerFailure(scenario.Logs, job);

        await scenario.RunJobAsync(job);

        (await scenario.State.FeedCheckpointAsync(unavailable, job)).Should().BeNull();
        (await scenario.State.FeedCheckpointAsync(available, job)).Should().NotBeNull();
    }

    [Theory]
    [InlineData(UserNotifierJob.Reddit)]
    [InlineData(UserNotifierJob.Youtube)]
    [InlineData(UserNotifierJob.Tumblr)]
    public async Task MalformedFeed_ReportsFailureWithoutAdvancingCheckpoint(UserNotifierJob job)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.FeedAsync(guild, job);
        scenario.Feeds.MalformedPosts(job);
        FeedFixtures.ExpectCheckerFailure(scenario.Logs, job);

        var output = await scenario.RunJobAsync(job);

        output.Messages.Should().BeEmpty();
        (await scenario.State.FeedCheckpointAsync(guild, job)).Should().BeNull();
    }

    [Theory]
    [InlineData(UserNotifierJob.Reddit)]
    [InlineData(UserNotifierJob.Youtube)]
    public async Task OlderPost_DoesNotReplaceNewerCheckpoint(UserNotifierJob job)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.FeedAsync(guild, job);
        await scenario.Given.NewerFeedCheckpointAsync(guild, job);
        scenario.Feeds.Posts(job);

        var output = await scenario.RunJobAsync(job);

        output.Messages.Should().BeEmpty();
        (await scenario.State.FeedCheckpointAsync(guild, job)).Should().Be("newer-item");
    }
}
