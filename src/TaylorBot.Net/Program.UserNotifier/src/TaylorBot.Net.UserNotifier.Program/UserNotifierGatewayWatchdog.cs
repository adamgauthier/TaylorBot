using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Program;

namespace TaylorBot.Net.UserNotifier.Program;

public sealed partial class UserNotifierGatewayWatchdog(
    DiscordGatewayHealthService health,
    TimeProvider timeProvider,
    IHostApplicationLifetime lifetime,
    ILogger<UserNotifierGatewayWatchdog> logger) : BackgroundService
{
    private static readonly TimeSpan FailureTimeout = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);
    private readonly Lock _lock = new();
    private TaskCompletionSource _checked = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private long _checkVersion;
    private int _exitCode;

    public int ExitCode => Volatile.Read(ref _exitCode);
    public long CheckVersion => Volatile.Read(ref _checkVersion);

    public Task WaitForCheckAsync(long version, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return version == _checkVersion ? _checked.Task.WaitAsync(cancellationToken) : Task.CompletedTask;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await health.WaitUntilStartedAsync(stoppingToken);
        using PeriodicTimer timer = new(CheckInterval, timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                CheckHealth();
            }
            finally
            {
                lock (_lock)
                {
                    _checkVersion++;
                    var previous = _checked;
                    _checked = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    previous.TrySetResult();
                }
            }
            if (lifetime.ApplicationStopping.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private void CheckHealth()
    {
        if (lifetime.ApplicationStopping.IsCancellationRequested)
        {
            return;
        }
        var snapshot = health.GetSnapshot();
        var failed = snapshot.Shards.Where(shard => shard.HasBeenHealthy
            ? !shard.Healthy && shard.UnhealthyFor >= FailureTimeout
            : snapshot.StartupAge >= FailureTimeout).ToArray();
        if (failed.Length == 0 && (snapshot.Shards.Count != 0 || snapshot.StartupAge < FailureTimeout))
        {
            return;
        }
        if (lifetime.ApplicationStopping.IsCancellationRequested || Interlocked.CompareExchange(ref _exitCode, value: 1, comparand: 0) != 0)
        {
            return;
        }
        var details = failed.Length == 0 ? "No shards initialized" : string.Join("; ", failed.Select(shard =>
            $"Shard #{shard.ShardId}: State={shard.ConnectionState}, InitialHealthEstablished={shard.HasBeenHealthy}, UnhealthySeconds={shard.UnhealthyFor.TotalSeconds}, HeartbeatAgeSeconds={shard.HeartbeatAge.TotalSeconds}, LastDisconnectError={shard.LastDisconnectError}"));
        LogGatewayFailure(snapshot.StartupAge.TotalSeconds, details);
        lifetime.StopApplication();
    }

    [LoggerMessage(Level = LogLevel.Critical, Message = "Gateway recovery deadline exceeded; requesting graceful failure shutdown. StartupAgeSeconds={StartupAgeSeconds}, FailedShards={FailedShards}")]
    private partial void LogGatewayFailure(double startupAgeSeconds, string failedShards);
}
