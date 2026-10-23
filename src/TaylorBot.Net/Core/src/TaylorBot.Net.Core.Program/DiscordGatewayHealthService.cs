using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.Core.Program;

public sealed partial class DiscordGatewayHealthService(
    TimeProvider timeProvider,
    ILogger<DiscordGatewayHealthService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan SummaryInterval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan HeartbeatWarningAge = TimeSpan.FromMinutes(3);

    private readonly Lock _lock = new();
    private readonly Dictionary<int, ShardHealth> _health = [];
    private readonly TaskCompletionSource _started = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private DiscordShardedClient? _gateway;
    private long _startedAt;

    public void Observe(DiscordShardedClient gateway)
    {
        lock (_lock)
        {
            if (_gateway != null)
            {
                throw new InvalidOperationException("Gateway health observation has already started.");
            }
            _gateway = gateway;
            _startedAt = timeProvider.GetTimestamp();
            _gateway.ShardConnected += OnConnectedAsync;
            _gateway.ShardReady += OnReadyAsync;
            _gateway.ShardDisconnected += OnDisconnectedAsync;
            _gateway.ShardLatencyUpdated += OnLatencyUpdatedAsync;
        }
        _started.TrySetResult();
    }

    public Task WaitUntilStartedAsync(CancellationToken cancellationToken) => _started.Task.WaitAsync(cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WaitUntilStartedAsync(stoppingToken);
        using PeriodicTimer timer = new(CheckInterval, timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var shard in GetSnapshot().Shards)
            {
                CheckHealth(shard);
            }
        }
    }

    public Snapshot GetSnapshot()
    {
        lock (_lock)
        {
            var gateway = _gateway ?? throw new InvalidOperationException("Gateway health observation has not started.");
            var now = timeProvider.GetTimestamp();
            var initialized = gateway.Shards;
            var shards = (initialized ?? []).OfType<DiscordSocketClient>().ToDictionary(shard => shard.ShardId);
            List<ShardSnapshot> snapshots = [];
            foreach (var id in Enumerable.Range(0, initialized?.Count ?? 0).Union(shards.Keys).Union(_health.Keys).ToArray())
            {
                var shard = shards.GetValueOrDefault(id);
                var health = GetHealth(id);
                var state = shard?.ConnectionState ?? ConnectionState.Disconnected;
                var (healthy, heartbeatAge) = GetHealthStatus(health, state, now);
                UpdateHealth(health, healthy, now);
                var unhealthyFor = health.UnhealthySince == null ? TimeSpan.Zero : timeProvider.GetElapsedTime(health.UnhealthySince.Value, now);
                if (!healthy && heartbeatAge > HeartbeatWarningAge && heartbeatAge - HeartbeatWarningAge > unhealthyFor)
                {
                    unhealthyFor = heartbeatAge - HeartbeatWarningAge;
                }
                snapshots.Add(new(id, healthy, health.HasBeenHealthy, unhealthyFor, state, shard?.Latency ?? 0, heartbeatAge,
                    health.LastHeartbeatAt, health.LastReadyAt, health.LastDisconnectedAt, health.LastDisconnectError));
            }
            return new(timeProvider.GetElapsedTime(_startedAt, now), snapshots);
        }
    }

    private void CheckHealth(ShardSnapshot shard)
    {
        lock (_lock)
        {
            var health = GetHealth(shard.ShardId);
            var now = timeProvider.GetTimestamp();
            if (health.LastReportedHealthy == shard.Healthy && health.LastReportedAt != null &&
                timeProvider.GetElapsedTime(health.LastReportedAt.Value, now) < SummaryInterval)
            {
                return;
            }
            health.LastReportedAt = now;
            health.LastReportedHealthy = shard.Healthy;
        }
        LogShardHealth(shard.Healthy ? LogLevel.Information : LogLevel.Warning, shard.ShardId, shard.Healthy, shard.ConnectionState,
            shard.Latency, shard.LastHeartbeatAt, shard.HeartbeatAge.TotalSeconds, shard.LastReadyAt, shard.LastDisconnectedAt, shard.LastDisconnectError);
    }

    private ShardHealth GetHealth(int id)
    {
        if (!_health.TryGetValue(id, out var health))
        {
            _health.Add(id, health = new() { UnhealthySince = _startedAt });
        }
        return health;
    }

    private static void UpdateHealth(ShardHealth health, bool healthy, long now)
    {
        health.UnhealthySince = healthy ? null : health.UnhealthySince ?? now;
        health.HasBeenHealthy |= healthy && health.LastHeartbeatTimestamp != null;
    }

    private void RefreshHealth(DiscordSocketClient shard, ShardHealth health)
    {
        var now = timeProvider.GetTimestamp();
        var (healthy, _) = GetHealthStatus(health, shard.ConnectionState, now);
        UpdateHealth(health, healthy, now);
    }

    private (bool Healthy, TimeSpan HeartbeatAge) GetHealthStatus(ShardHealth health, ConnectionState state, long now)
    {
        var age = timeProvider.GetElapsedTime(health.LastHeartbeatTimestamp ?? _startedAt, now);
        return (state == ConnectionState.Connected && health.LastReadyAt != null && age <= HeartbeatWarningAge, age);
    }

    private Task OnConnectedAsync(DiscordSocketClient shard)
    {
        lock (_lock)
        {
            RefreshHealth(shard, GetHealth(shard.ShardId));
        }
        return Task.CompletedTask;
    }

    private Task OnReadyAsync(DiscordSocketClient shard)
    {
        lock (_lock)
        {
            var health = GetHealth(shard.ShardId);
            health.LastReadyAt = timeProvider.GetUtcNow();
            RefreshHealth(shard, health);
        }
        return Task.CompletedTask;
    }

    private Task OnDisconnectedAsync(Exception? exception, DiscordSocketClient shard)
    {
        lock (_lock)
        {
            var health = GetHealth(shard.ShardId);
            health.LastDisconnectedAt = timeProvider.GetUtcNow();
            health.LastDisconnectError = exception?.GetBaseException().GetType().Name;
            health.UnhealthySince ??= timeProvider.GetTimestamp();
        }
        return Task.CompletedTask;
    }

    private Task OnLatencyUpdatedAsync(int previous, int current, DiscordSocketClient shard)
    {
        lock (_lock)
        {
            var health = GetHealth(shard.ShardId);
            health.LastHeartbeatAt = timeProvider.GetUtcNow();
            health.LastHeartbeatTimestamp = timeProvider.GetTimestamp();
            RefreshHealth(shard, health);
        }
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await base.StopAsync(cancellationToken);
        }
        finally
        {
            Unsubscribe();
        }
    }

    public override void Dispose()
    {
        Unsubscribe();
        base.Dispose();
    }

    private void Unsubscribe()
    {
        if (_gateway != null)
        {
            _gateway.ShardConnected -= OnConnectedAsync;
            _gateway.ShardReady -= OnReadyAsync;
            _gateway.ShardDisconnected -= OnDisconnectedAsync;
            _gateway.ShardLatencyUpdated -= OnLatencyUpdatedAsync;
        }
    }

    public sealed record Snapshot(TimeSpan StartupAge, IReadOnlyList<ShardSnapshot> Shards);

    public sealed record ShardSnapshot(int ShardId, bool Healthy, bool HasBeenHealthy, TimeSpan UnhealthyFor,
        ConnectionState ConnectionState, int Latency, TimeSpan HeartbeatAge, DateTimeOffset? LastHeartbeatAt,
        DateTimeOffset? LastReadyAt, DateTimeOffset? LastDisconnectedAt, string? LastDisconnectError);

    private sealed class ShardHealth
    {
        public DateTimeOffset? LastReadyAt { get; set; }
        public DateTimeOffset? LastHeartbeatAt { get; set; }
        public long? LastHeartbeatTimestamp { get; set; }
        public DateTimeOffset? LastDisconnectedAt { get; set; }
        public string? LastDisconnectError { get; set; }
        public long? UnhealthySince { get; set; }
        public bool HasBeenHealthy { get; set; }
        public long? LastReportedAt { get; set; }
        public bool? LastReportedHealthy { get; set; }
    }

    [LoggerMessage(Message = "Gateway health: Shard #{ShardId}, Healthy={Healthy}, State={ConnectionState}, LatencyMs={LatencyMs}, LastHeartbeatAt={LastHeartbeatAt}, HeartbeatAgeSeconds={HeartbeatAgeSeconds}, LastReadyAt={LastReadyAt}, LastDisconnectedAt={LastDisconnectedAt}, LastDisconnectError={LastDisconnectError}")]
    private partial void LogShardHealth(LogLevel level, int shardId, bool healthy, ConnectionState connectionState, int latencyMs,
        DateTimeOffset? lastHeartbeatAt, double heartbeatAgeSeconds, DateTimeOffset? lastReadyAt,
        DateTimeOffset? lastDisconnectedAt, string? lastDisconnectError);
}
