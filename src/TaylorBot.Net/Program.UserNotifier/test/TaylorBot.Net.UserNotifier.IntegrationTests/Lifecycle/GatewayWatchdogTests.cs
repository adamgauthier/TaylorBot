using TaylorBot.Net.UserNotifier.IntegrationTests.Discord;
using TaylorBot.Net.UserNotifier.IntegrationTests.ExternalApis;
using TaylorBot.Net.UserNotifier.Program;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Lifecycle;

public sealed class GatewayWatchdogTests(DataServices data)
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task DisconnectedShard_ExitsAtDeadlineEvenWhileOtherShardsAndJobsAreHealthy(int shards)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:ShardCount"] = $"{shards}" }, runUntilShutdown: true);
        scenario.GatewayHealth.Shards.Should().OnlyContain(shard => shard.Healthy && shard.HasBeenHealthy);
        await scenario.StopGatewayAsync(shardId: 0);

        await scenario.AdvanceAsync(TimeSpan.FromMinutes(14) + TimeSpan.FromSeconds(59));

        scenario.ShutdownRequested.Should().BeFalse();
        scenario.Jobs.Jobs.Should().OnlyContain(job => !job.IsRunning && job.NextRunAt > scenario.Now);
        if (shards > 1)
        {
            scenario.GatewayHealth.Shards.Where(shard => shard.ShardId != 0).Should().OnlyContain(shard => shard.Healthy);
        }
        scenario.Logs.ExpectError<UserNotifierGatewayWatchdog>("Gateway recovery deadline exceeded");

        await scenario.AdvanceAsync(TimeSpan.FromSeconds(1));
        (await scenario.WaitForExitAsync()).Should().Be(1);

        scenario.Logs.ToString().Should().Contain("Shard #0: State=Disconnected");
        scenario.Logs.ToString().Split('\n').Count(line => line.Contains("Gateway recovery deadline exceeded", StringComparison.Ordinal)).Should().Be(1);
        scenario.Jobs.Jobs.Should().OnlyContain(job => !job.IsRunning && job.NextRunAt == null);
        var before = scenario.Logs.ToString();
        await scenario.AdvanceAsync(TimeSpan.FromMinutes(30));
        scenario.Logs.ToString().Should().Be(before);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    public async Task MissingReady_UsesStartupDeadlineWithoutExtraGrace(int shards, int heldShard)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?>
            {
                ["Discord:ShardCount"] = $"{shards}",
                ["TestGateway:HoldReadyForShard"] = $"{heldShard}",
            }, runUntilShutdown: true, waitForReady: false);
        if (shards > 1)
        {
            await scenario.WaitForShardReadyAsync(shardId: 0);
        }

        await scenario.AdvanceAsync(TimeSpan.FromMinutes(14) + TimeSpan.FromSeconds(59));

        scenario.ShutdownRequested.Should().BeFalse();
        scenario.GatewayHealth.Shards.Single(shard => shard.ShardId == heldShard).HasBeenHealthy.Should().BeFalse();
        scenario.Logs.ExpectError<UserNotifierGatewayWatchdog>("Gateway recovery deadline exceeded");

        await scenario.AdvanceAsync(TimeSpan.FromSeconds(1));
        (await scenario.WaitForExitAsync()).Should().Be(1);

        scenario.Logs.ToString().Should().Contain($"Shard #{heldShard}:").And.Contain("InitialHealthEstablished=False");
    }

    [Fact]
    public async Task ReadyBeforeDeadline_PreventsStartupFailure()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["TestGateway:HoldReadyForShard"] = "0" },
            runUntilShutdown: true, waitForReady: false);
        await scenario.AdvanceAsync(TimeSpan.FromMinutes(14) + TimeSpan.FromSeconds(59));

        await scenario.RepeatReadyAsync();
        await scenario.AdvanceAsync(TimeSpan.FromSeconds(1));

        scenario.ShutdownRequested.Should().BeFalse();
        scenario.GatewayHealth.Shards.Should().ContainSingle(shard => shard.Healthy && shard.HasBeenHealthy);
        await scenario.StopAsync();
        (await scenario.WaitForExitAsync()).Should().Be(0);
    }

    [Fact]
    public async Task MultipleFailedShards_RequestOneShutdown()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:ShardCount"] = "2" }, runUntilShutdown: true);
        await scenario.StopGatewayAsync();
        scenario.Logs.ExpectError<UserNotifierGatewayWatchdog>("Gateway recovery deadline exceeded");

        await scenario.AdvanceAsync(TimeSpan.FromMinutes(15));

        (await scenario.WaitForExitAsync()).Should().Be(1);
        scenario.Logs.ToString().Should().Contain("Shard #0:").And.Contain("Shard #1:");
        scenario.Logs.ToString().Split('\n').Count(line => line.Contains("Gateway recovery deadline exceeded", StringComparison.Ordinal)).Should().Be(1);
    }

    [Fact]
    public async Task UninitializedShards_AbortBlockedStartupAtDeadline()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["TestGateway:HoldShardInitialization"] = "true" },
            runUntilShutdown: true, waitForReady: false);
        scenario.GatewayHealth.Shards.Should().BeEmpty();
        await scenario.AdvanceAsync(TimeSpan.FromMinutes(14) + TimeSpan.FromSeconds(59));
        scenario.ShutdownRequested.Should().BeFalse();
        scenario.Logs.ExpectError<UserNotifierGatewayWatchdog>("Gateway recovery deadline exceeded");
        scenario.Logs.ExpectError("Microsoft.Extensions.Hosting.Internal.Host", "Hosting failed to start");

        await scenario.AdvanceAsync(TimeSpan.FromSeconds(1));

        (await scenario.WaitForExitAsync()).Should().Be(1);
        scenario.Logs.ToString().Should().Contain("No shards initialized");
        scenario.DiscordApi.GatewayInitializationCancelled.Should().BeTrue();
    }

    [Fact]
    public async Task ReadyWithoutHeartbeat_DoesNotEstablishInitialHealth()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["TestGateway:HoldHeartbeatAcknowledgements"] = "true" },
            automaticHeartbeats: false, runUntilShutdown: true);

        await scenario.AdvanceAsync(TimeSpan.FromMinutes(14) + TimeSpan.FromSeconds(59));
        scenario.ShutdownRequested.Should().BeFalse();
        scenario.Logs.ExpectError<UserNotifierGatewayWatchdog>("Gateway recovery deadline exceeded");

        await scenario.AdvanceAsync(TimeSpan.FromSeconds(1));

        (await scenario.WaitForExitAsync()).Should().Be(1);
        scenario.Logs.ToString().Should().Contain("InitialHealthEstablished=False");
    }

    [Fact]
    public async Task StaleHeartbeat_ExitsAfterFreshnessAllowanceAndContinuousFailureWindow()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            automaticHeartbeats: false, runUntilShutdown: true);
        scenario.GatewayHealth.Shards.Should().OnlyContain(shard => shard.Healthy && shard.HasBeenHealthy);

        await scenario.AdvanceAsync(TimeSpan.FromMinutes(17) + TimeSpan.FromSeconds(59));
        scenario.ShutdownRequested.Should().BeFalse();
        scenario.Logs.ExpectError<UserNotifierGatewayWatchdog>("Gateway recovery deadline exceeded");

        await scenario.AdvanceAsync(TimeSpan.FromSeconds(1));

        (await scenario.WaitForExitAsync()).Should().Be(1);
        scenario.Logs.ToString().Should().Contain("State=Connected").And.Contain("UnhealthySeconds=900");
    }

    [Fact]
    public async Task HeartbeatRecovery_ResetsFailureWindowEvenBetweenChecks()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            automaticHeartbeats: false, runUntilShutdown: true);
        await scenario.AdvanceAsync(TimeSpan.FromMinutes(17));

        await scenario.AcknowledgeHeartbeatAsync();
        await scenario.AdvanceAsync(TimeSpan.FromMinutes(17));

        scenario.ShutdownRequested.Should().BeFalse();
        scenario.Logs.ExpectError<UserNotifierGatewayWatchdog>("Gateway recovery deadline exceeded");
        await scenario.AdvanceAsync(TimeSpan.FromMinutes(1));

        (await scenario.WaitForExitAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SessionResume_ResetsDisconnectWindowWithoutAnotherReady()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            runUntilShutdown: true);
        var ready = scenario.GatewayHealth.Shards.Single().LastReadyAt;
        await scenario.StopGatewayAsync();
        await scenario.AdvanceAsync(TimeSpan.FromMinutes(14));

        await scenario.ResumeGatewayAsync();

        scenario.GatewayHealth.Shards.Should().ContainSingle(shard => shard.Healthy && shard.LastReadyAt == ready);
        await scenario.StopGatewayAsync();
        await scenario.AdvanceAsync(TimeSpan.FromMinutes(14) + TimeSpan.FromSeconds(59));
        scenario.ShutdownRequested.Should().BeFalse();
        scenario.Logs.ExpectError<UserNotifierGatewayWatchdog>("Gateway recovery deadline exceeded");
        await scenario.AdvanceAsync(TimeSpan.FromSeconds(1));

        (await scenario.WaitForExitAsync()).Should().Be(1);
    }

    [Fact]
    public async Task FailureShutdown_DrainsActiveRequestAndFlushesPendingTracking()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            job: UserNotifierJob.Reddit, runUntilShutdown: true);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var message = await scenario.Discord.MessageAsync(guild, user, "Pending tracking");
        await scenario.Given.FeedAsync(guild, UserNotifierJob.Reddit);
        scenario.Feeds.Posts(UserNotifierJob.Reddit);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        using var response = scenario.External.Pause(FeedFixtures.RedditListingUri);
        await scenario.RunUntilRequestAsync(UserNotifierJob.Reddit, response);
        await scenario.StopGatewayAsync();
        scenario.Logs.ExpectError<UserNotifierGatewayWatchdog>("Gateway recovery deadline exceeded");

        await scenario.AdvanceWhileRequestBlockedAsync(UserNotifierJob.Reddit, TimeSpan.FromMinutes(15));

        scenario.ShutdownRequested.Should().BeTrue();
        scenario.Jobs.Jobs.Single(job => job.Name == UserNotifierJob.Reddit).IsRunning.Should().BeTrue();
        var queued = message with { Timestamp = message.Timestamp.AddMinutes(30) };
        await scenario.Given.QueueLegacyLastSpokeAsync(queued);
        response.Dispose();
        (await scenario.WaitForExitAsync()).Should().Be(1);

        scenario.Jobs.Jobs.Single(job => job.Name == UserNotifierJob.Reddit).CompletedCycles.Should().Be(1);
        (await scenario.State.MemberAsync(guild, user)).Messages.Should().Be(1);
        (await scenario.State.MemberAsync(guild, user)).LastSpokeAt.Should().BeCloseTo(queued.Timestamp.UtcDateTime, TimeSpan.FromMilliseconds(1));
        (await scenario.State.LastSpokeQueueFieldsAsync(pending: true)).Should().BeEmpty();
        (await scenario.State.LastSpokeQueueFieldsAsync()).Should().BeEmpty();
        (await scenario.State.FeedCheckpointAsync(guild, UserNotifierJob.Reddit)).Should().Be("post123");
    }

    [Fact]
    public async Task NormalShutdown_ReturnsSuccessAndDoesNotTriggerRecovery()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            runUntilShutdown: true);
        await scenario.StopGatewayAsync();

        await scenario.StopAsync();
        (await scenario.WaitForExitAsync()).Should().Be(0);
        await scenario.AdvanceAsync(TimeSpan.FromHours(1));

        scenario.ExitCode.Should().Be(0);
        scenario.Logs.ToString().Should().NotContain("Gateway recovery deadline exceeded");
    }
}
