namespace TaylorBot.Net.UserNotifier.IntegrationTests.Lifecycle;

public sealed class GatewayHealthTests(DataServices data)
{
    [Fact]
    public async Task Summaries_ReportEveryShardWithoutMessagePayloads()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:ShardCount"] = "2" }, gatewayHealthLogs: true);
        await scenario.RepeatReadyAsync();
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        const string content = "Private sample message for the health logging scenario";
        await scenario.Discord.MessageAsync(guild, user, content);

        await scenario.AdvanceAsync(TimeSpan.FromSeconds(59));
        scenario.Logs.ToString().Should().NotContain("Gateway health:");
        await scenario.AdvanceAsync(TimeSpan.FromSeconds(1));
        await scenario.WaitForLogAsync("Gateway health: Shard #0");
        await scenario.WaitForLogAsync("Gateway health: Shard #1");

        var summaries = scenario.Logs.ToString().Split('\n').Where(line => line.Contains("Gateway health:", StringComparison.Ordinal)).ToArray();
        summaries.Should().HaveCount(2);
        summaries.Should().OnlyContain(line => line.Contains("Information", StringComparison.Ordinal) &&
            line.Contains("State=Connected", StringComparison.Ordinal) &&
            line.Contains("LatencyMs=", StringComparison.Ordinal) &&
            line.Contains("LastReadyAt=", StringComparison.Ordinal));
        summaries.Should().Contain(line => line.Contains("Shard #0", StringComparison.Ordinal));
        summaries.Should().Contain(line => line.Contains("Shard #1", StringComparison.Ordinal));
        summaries.Should().OnlyContain(line => !line.Contains(content, StringComparison.Ordinal));
    }

    [Fact]
    public async Task DisconnectedGateway_ProducesWarningsWhileJobsRemainAlive()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, gatewayHealthLogs: true);
        await scenario.StopGatewayAsync();

        await scenario.AdvanceAsync(TimeSpan.FromMinutes(1));
        await scenario.WaitForLogAsync("Gateway health: Shard #0");

        var summary = scenario.Logs.ToString().Split('\n').Single(line => line.Contains("Gateway health:", StringComparison.Ordinal));
        summary.Should().Contain("Warning").And.Contain("State=Disconnected").And.Contain("LastDisconnectedAt=");
        scenario.Jobs.Jobs.Should().OnlyContain(job => !job.IsRunning && job.NextRunAt > scenario.Now);
    }

    [Fact]
    public async Task UnchangedHealth_ReportsAgainOnlyAfterSummaryInterval()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, gatewayHealthLogs: true);
        await scenario.AdvanceAsync(TimeSpan.FromMinutes(1));
        await scenario.WaitForLogAsync("Gateway health: Shard #0");

        await scenario.AdvanceAsync(TimeSpan.FromMinutes(14));

        scenario.Logs.ToString().Split('\n').Count(line => line.Contains("Gateway health:", StringComparison.Ordinal)).Should().Be(1);

        await scenario.AdvanceAsync(TimeSpan.FromMinutes(1));
        await scenario.WaitForLogAsync("Gateway health: Shard #0", count: 2);

        var summaries = scenario.Logs.ToString().Split('\n').Where(line => line.Contains("Gateway health:", StringComparison.Ordinal)).ToArray();
        summaries.Should().HaveCount(2).And.OnlyContain(line => line.Contains("Information", StringComparison.Ordinal));
    }

    [Fact]
    public async Task HealthTransitions_ReportFailureAndRecoveryWithoutWaitingForSummaryInterval()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["TestGateway:HoldHeartbeatAcknowledgements"] = "true" },
            gatewayHealthLogs: true, automaticHeartbeats: false);
        await scenario.AdvanceAsync(TimeSpan.FromMinutes(1));
        await scenario.WaitForLogAsync("Gateway health: Shard #0");
        await scenario.AdvanceAsync(TimeSpan.FromMinutes(1));
        scenario.Logs.ToString().Split('\n').Count(line => line.Contains("Gateway health:", StringComparison.Ordinal)).Should().Be(1);

        await scenario.AdvanceAsync(TimeSpan.FromMinutes(2));
        await scenario.WaitForLogAsync("Gateway health: Shard #0", count: 2);
        await scenario.AcknowledgeHeartbeatAsync();
        await scenario.AdvanceAsync(TimeSpan.FromMinutes(1));
        await scenario.WaitForLogAsync("Gateway health: Shard #0", count: 3);

        var summaries = scenario.Logs.ToString().Split('\n').Where(line => line.Contains("Gateway health:", StringComparison.Ordinal)).ToArray();
        summaries.Should().HaveCount(3);
        summaries[0].Should().Contain("Information").And.Contain("Healthy=True");
        summaries[1].Should().Contain("Warning").And.Contain("Healthy=False").And.Contain("HeartbeatAgeSeconds=240");
        summaries[2].Should().Contain("Information").And.Contain("Healthy=True").And.Contain("HeartbeatAgeSeconds=60");
    }

    [Theory]
    [InlineData(3, "Information")]
    [InlineData(4, "Warning")]
    public async Task MissingHeartbeat_WarnsAfterGracePeriodEvenWhenSocketRemainsConnected(int minutes, string level)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            gatewayHealthLogs: true, automaticHeartbeats: false);

        await scenario.AdvanceAsync(TimeSpan.FromMinutes(minutes));
        await scenario.WaitForLogAsync("Gateway health: Shard #0");

        var summary = scenario.Logs.ToString().Split('\n').Last(line => line.Contains("Gateway health:", StringComparison.Ordinal));
        summary.Should().Contain(level).And.Contain("State=Connected").And.Contain($"HeartbeatAgeSeconds={minutes * 60}");
    }

    [Fact]
    public async Task Shutdown_StopsPeriodicSummaries()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, gatewayHealthLogs: true);
        await scenario.StopAsync();
        var before = scenario.Logs.ToString();

        await scenario.AdvanceAsync(TimeSpan.FromMinutes(5));

        scenario.Logs.ToString().Should().Be(before);
    }
}
