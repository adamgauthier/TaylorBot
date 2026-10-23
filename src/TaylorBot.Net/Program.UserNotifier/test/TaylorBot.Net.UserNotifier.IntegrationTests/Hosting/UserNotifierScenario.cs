using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.IntegrationTests.Shared.ExternalApis;
using TaylorBot.Net.IntegrationTests.Shared.Hosting;
using TaylorBot.Net.UserNotifier.IntegrationTests.Discord;
using TaylorBot.Net.UserNotifier.IntegrationTests.Discord.DiscordNet;
using TaylorBot.Net.UserNotifier.IntegrationTests.ExternalApis;
using TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;
using TaylorBot.Net.UserNotifier.Program;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Hosting;

public sealed class UserNotifierScenario : IAsyncDisposable
{
    private readonly DataServices _services;
    private readonly ScenarioDatabase _database;
    private readonly CancellationToken _cancellationToken;
    private readonly ScenarioClock _time = new();
    private readonly NotifierGateway _gateway;
    private IHost? _host;
    private Task? _shutdown;
    private Task<int>? _run;
    private bool _automaticHeartbeats;
    private bool _stopped;
    private bool _disposed;

    public NotifierDiscordApi DiscordApi { get; } = new();
    public ExternalApi External { get; } = new();
    public ScenarioLogs Logs { get; } = new();
    public ScenarioData Given { get; }
    public ScenarioState State { get; }
    public NotifierDriver Discord { get; }
    public FeedFixtures Feeds { get; }
    public PatreonFixtures Patreon { get; }
    public JobsStatus Jobs => _host!.Services.GetRequiredService<UserNotifierJobs>().GetStatus();
    public DateTimeOffset Now => _time.GetUtcNow();
    public bool ShutdownRequested => _host!.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.IsCancellationRequested;
    public int ExitCode => _host!.Services.GetRequiredService<UserNotifierGatewayWatchdog>().ExitCode;
    public DiscordGatewayHealthService.Snapshot GatewayHealth => _host!.Services.GetRequiredService<DiscordGatewayHealthService>().GetSnapshot();

    private UserNotifierScenario(DataServices services, ScenarioDatabase database, CancellationToken cancellationToken)
    {
        _services = services;
        _database = database;
        _cancellationToken = cancellationToken;
        _gateway = new(DiscordApi);

        Given = new(database, DiscordApi, DispatchAsync);
        State = new(database);
        Discord = new(DiscordApi, DispatchAsync);
        Feeds = new(External);
        Patreon = new(External);
    }

    public static async Task<UserNotifierScenario> CreateAsync(DataServices services, CancellationToken cancellationToken,
        UserNotifierJob? job = null, IReadOnlyDictionary<string, string?>? settings = null, bool gatewayHealthLogs = false,
        bool automaticHeartbeats = true, bool runUntilShutdown = false, bool waitForReady = true)
    {
        UserNotifierScenario scenario = new(services, await services.CreateDatabaseAsync(), cancellationToken);

        try
        {
            scenario._automaticHeartbeats = automaticHeartbeats;
            await scenario.StartAsync(job, settings, gatewayHealthLogs, runUntilShutdown, waitForReady);
            return scenario;
        }
        catch (Exception startupFailure)
        {
            try
            {
                await scenario.DisposeAsync();
            }
            catch (Exception cleanupFailure)
            {
                throw new AggregateException("Notifier startup and cleanup failed.", startupFailure, cleanupFailure);
            }

            throw;
        }
    }

    private async Task StartAsync(UserNotifierJob? job, IReadOnlyDictionary<string, string?>? settings, bool gatewayHealthLogs,
        bool runUntilShutdown, bool waitForReady)
    {
        var configuration = ScenarioConfiguration.Create(_services, new NpgsqlConnectionStringBuilder(_database.ConnectionString), job);
        configuration["TestGateway:HeartbeatIntervalMilliseconds"] = _automaticHeartbeats ? "100" : "1000";
        if (settings != null)
        {
            foreach (var setting in settings)
            {
                configuration[setting.Key] = setting.Value;
            }
            DiscordApi.HoldGatewayInitialization = bool.Parse(configuration.GetValueOrDefault("TestGateway:HoldShardInitialization") ?? "false");
        }

        _host = UserNotifierProgram.CreateHostBuilder(new HostBuilder()
            .ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(configuration))
            .ConfigureLogging(builder => builder.AddProvider(Logs).SetMinimumLevel(LogLevel.Debug)
                .AddFilter<ScenarioLogs>(typeof(DiscordGatewayHealthService).FullName,
                    gatewayHealthLogs ? LogLevel.Debug : LogLevel.Error)))
            .ConfigureServices(services =>
            {
                services.Replace(ServiceDescriptor.Singleton<TimeProvider>(_time));
                _gateway.Configure(services);
                NotifierTransports.Configure(services, External);
            })
            .Build();

        using var deadline = Deadline();
        if (runUntilShutdown)
        {
            TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
            using var registration = _host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.Register(() => started.TrySetResult());
            _run = UserNotifierProgram.RunAsync(_host, _cancellationToken);
            if (DiscordApi.HoldGatewayInitialization)
            {
                await DiscordApi.WaitForInitializationAsync(deadline.Token);
                await _time.WaitForPeriodicTimersAsync(count: 1, deadline.Token);
                return;
            }
            await Task.WhenAny(started.Task, _run).WaitAsync(deadline.Token);
            if (_run.IsCompleted)
            {
                await _run;
                throw new InvalidOperationException("Notifier exited before startup completed.");
            }
        }
        else
        {
            await _host.StartAsync(deadline.Token);
        }
        await _time.WaitForPeriodicTimersAsync(count: 2, deadline.Token);
        if (waitForReady)
        {
            await _gateway.WaitUntilReadyAsync(deadline.Token);
        }
        else
        {
            await _gateway.WaitUntilIdentifiedAsync(deadline.Token);
        }
        if (!bool.Parse(configuration.GetValueOrDefault("TestGateway:HoldHeartbeatAcknowledgements") ?? "false"))
        {
            await _gateway.AcknowledgeHeartbeatAsync(deadline.Token);
        }
        if (!_automaticHeartbeats)
        {
            _gateway.HoldHeartbeats();
        }
        await SettleAsync(deadline.Token);
    }

    private CancellationTokenSource Deadline()
    {
        var deadline = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        return deadline;
    }

    private async Task DispatchAsync(string name, object data)
    {
        using var deadline = Deadline();
        await _gateway.DispatchAsync(name, data, deadline.Token);
        await SettleAsync(deadline.Token);
    }

    private async Task SettleAsync(CancellationToken cancellationToken)
    {
        var host = _host ?? throw new InvalidOperationException("Notifier has not started.");
        var jobs = host.Services.GetRequiredService<UserNotifierJobs>();

        while (true)
        {
            var drain = host.Services.GetRequiredService<BackgroundTasks>().DrainAsync(cancellationToken);

            DiscordApi.EnsureNoUnexpectedRequests();
            External.EnsureNoUnexpectedRequests();
            Logs.EnsureNoErrors();

            var drained = drain.IsCompleted;
            var status = jobs.GetStatus();
            if (drained && (status.Jobs.Count == 0 || status.Jobs.Count == Enum.GetValues<UserNotifierJob>().Length) &&
                status.Jobs.All(job => !job.IsRunning && job.NextRunAt > _time.GetUtcNow()))
            {
                await drain;
                return;
            }

            await WaitForProgressAsync(drain, status, cancellationToken, awaitCompletion: !drained);
        }
    }

    private async Task WaitForProgressAsync(Task completion, JobsStatus status, CancellationToken cancellationToken, bool awaitCompletion = false)
    {
        var jobs = _host!.Services.GetRequiredService<UserNotifierJobs>();
        var timers = _time.GetStatus();
        if (jobs.GetStatus().Version != status.Version)
        {
            return;
        }

        foreach (var waiting in status.Jobs.Where(job => job.NextRunAt != null))
        {
            timers.DueTimes.Remove(waiting.NextRunAt!.Value);
        }

        if (timers.DueTimes.Count > 0)
        {
            await AdvanceClockAsync(timers.DueTimes.Min() - _time.GetUtcNow(), cancellationToken);
            return;
        }

        var changed = Task.WhenAny(
            jobs.WaitForChangeAsync(status.Version, cancellationToken),
            _time.WaitForChangeAsync(timers.Version, cancellationToken));
        await (awaitCompletion || !completion.IsCompleted ? Task.WhenAny(changed, completion) : changed).WaitAsync(cancellationToken);
    }

    public async Task<DiscordOutput> AdvanceAsync(TimeSpan elapsed)
    {
        var start = DiscordApi.Requests.Count;
        using var deadline = Deadline();
        await AdvanceClockAsync(elapsed, deadline.Token);
        if (!_stopped && !ShutdownRequested)
        {
            await SettleAsync(deadline.Token);
        }
        return new([.. DiscordApi.Requests.Skip(start)]);
    }

    private async Task AdvanceClockAsync(TimeSpan elapsed, CancellationToken cancellationToken)
    {
        var watchdog = _host!.Services.GetRequiredService<UserNotifierGatewayWatchdog>();
        var version = watchdog.CheckVersion;
        var checkedHealth = await _time.AdvanceAsync(elapsed, () => RefreshGatewayHeartbeatAsync(cancellationToken));
        if (checkedHealth && !ShutdownRequested)
        {
            await watchdog.WaitForCheckAsync(version, cancellationToken);
        }
    }

    private async Task RefreshGatewayHeartbeatAsync(CancellationToken cancellationToken)
    {
        if (!_automaticHeartbeats || _stopped || ShutdownRequested)
        {
            return;
        }
        var stopping = _host!.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, stopping);
        try
        {
            await _gateway.AcknowledgeHeartbeatAsync(cancellation.Token);
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // Intentional shutdown can stop the SDK heartbeat while finite work is settling.
        }
    }

    public Task<DiscordOutput> RunJobAsync(UserNotifierJob job)
    {
        var status = _host!.Services.GetRequiredService<UserNotifierJobs>().GetStatus().Jobs.Single(status => status.Name == job);
        var due = status.NextRunAt ?? throw new InvalidOperationException($"{job} is not waiting for its next iteration.");
        return AdvanceAsync(due - _time.GetUtcNow());
    }

    public async Task RunCyclesAsync(UserNotifierJob job, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        for (var cycle = 0; cycle < count; cycle++)
        {
            await RunJobAsync(job);
        }
    }

    public async Task RepeatReadyAsync()
    {
        using var deadline = Deadline();
        await _gateway.RepeatReadyAsync(deadline.Token);
        await SettleAsync(deadline.Token);
    }

    public async Task WaitForShardReadyAsync(int shardId)
    {
        using var deadline = Deadline();
        await _gateway.WaitForShardReadyAsync(shardId, deadline.Token);
        await SettleAsync(deadline.Token);
    }

    public async Task StopGatewayAsync(int? shardId = null)
    {
        using var deadline = Deadline();
        var client = _host!.Services.GetRequiredService<ITaylorBotClient>();
        var remaining = client.DiscordShardedClient.Shards.Where(shard => shardId == null || shard.ShardId == shardId)
            .Select(shard => shard.ShardId).ToHashSet();
        Lock disconnectedLock = new();
        TaskCompletionSource disconnected = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task OnDisconnectedAsync(Exception? exception, DiscordSocketClient shard)
        {
            lock (disconnectedLock)
            {
                remaining.Remove(shard.ShardId);
                if (remaining.Count == 0)
                {
                    disconnected.TrySetResult();
                }
            }
            return Task.CompletedTask;
        }
        client.DiscordShardedClient.ShardDisconnected += OnDisconnectedAsync;
        try
        {
            await (shardId == null ? client.StopAsync().AsTask() : client.DiscordShardedClient.GetShard(shardId.Value).StopAsync()).WaitAsync(deadline.Token);
            await disconnected.Task.WaitAsync(deadline.Token);
        }
        finally
        {
            client.DiscordShardedClient.ShardDisconnected -= OnDisconnectedAsync;
        }
    }

    public async Task ResumeGatewayAsync(int shardId = 0)
    {
        using var deadline = Deadline();
        var shard = _host!.Services.GetRequiredService<ITaylorBotClient>().DiscordShardedClient.GetShard(shardId);
        TaskCompletionSource connected = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task OnConnectedAsync()
        {
            connected.TrySetResult();
            return Task.CompletedTask;
        }
        shard.Connected += OnConnectedAsync;
        try
        {
            await shard.StartAsync().WaitAsync(deadline.Token);
            await connected.Task.WaitAsync(deadline.Token);
            await AcknowledgeHeartbeatAsync();
        }
        finally
        {
            shard.Connected -= OnConnectedAsync;
        }
    }

    public async Task<int> WaitForExitAsync()
    {
        var run = _run ?? throw new InvalidOperationException("Scenario is not running the application entry-point lifecycle.");
        if (!ShutdownRequested)
        {
            throw new InvalidOperationException("Application shutdown has not been requested.");
        }
        await StopAsync();
        return await run;
    }

    public async Task AcknowledgeHeartbeatAsync()
    {
        using var deadline = Deadline();
        await _gateway.AcknowledgeHeartbeatAsync(deadline.Token);
        await SettleAsync(deadline.Token);
    }

    public async Task WaitForLogAsync(string message, int count = 1)
    {
        using var deadline = Deadline();
        await Logs.WaitForMessageAsync(message, deadline.Token, count);
    }

    public async Task ReloadAsync(string key, string value)
    {
        var configuration = _host!.Services.GetRequiredService<IConfiguration>();
        configuration[key] = value;
        ((IConfigurationRoot)configuration).Reload();

        using var deadline = Deadline();
        await SettleAsync(deadline.Token);
    }

    public async Task RunUntilPacingAsync(UserNotifierJob job)
    {
        var jobs = _host!.Services.GetRequiredService<UserNotifierJobs>();
        var before = Jobs.Jobs.Single(status => status.Name == job);
        using var deadline = Deadline();
        await AdvanceClockAsync(before.NextRunAt!.Value - Now, deadline.Token);
        while (true)
        {
            var status = jobs.GetStatus();
            var timers = _time.GetStatus();
            if (jobs.GetStatus().Version != status.Version)
            {
                continue;
            }

            foreach (var waiting in status.Jobs.Where(status => status.NextRunAt != null))
            {
                timers.DueTimes.Remove(waiting.NextRunAt!.Value);
            }

            var current = status.Jobs.Single(status => status.Name == job);
            if (current.IsRunning && timers.DueTimes.Count > 0)
            {
                return;
            }

            if (current.CompletedCycles > before.CompletedCycles)
            {
                throw new InvalidOperationException($"{job} completed without entering a pacing delay.");
            }

            await Task.WhenAny(jobs.WaitForChangeAsync(status.Version, deadline.Token),
                _time.WaitForChangeAsync(timers.Version, deadline.Token)).WaitAsync(deadline.Token);
        }
    }

    public async Task RunUntilRequestAsync(UserNotifierJob job, ExternalApi.ResponseGate gate)
    {
        var due = Jobs.Jobs.Single(status => status.Name == job).NextRunAt!.Value;
        using var deadline = Deadline();
        await AdvanceClockAsync(due - Now, deadline.Token);
        await gate.WaitForRequestAsync(deadline.Token);
    }

    public async Task AdvanceWhileRequestBlockedAsync(UserNotifierJob job, TimeSpan elapsed)
    {
        var jobs = _host!.Services.GetRequiredService<UserNotifierJobs>();
        using var deadline = Deadline();
        await AdvanceClockAsync(elapsed, deadline.Token);

        while (true)
        {
            var status = jobs.GetStatus();
            if (ShutdownRequested || status.Jobs.Where(status => status.Name != job).All(status => !status.IsRunning && status.NextRunAt > Now))
            {
                return;
            }

            await jobs.WaitForChangeAsync(status.Version, deadline.Token);
        }
    }

    public Task RequestStopAsync(CancellationToken cancellationToken)
    {
        if (_shutdown == null || _shutdown.IsCompleted && !_shutdown.IsCompletedSuccessfully)
        {
            if (_run != null)
            {
                _host!.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
                _shutdown = _run;
            }
            else
            {
                _shutdown = _host?.StopAsync(cancellationToken) ?? Task.CompletedTask;
            }
        }

        return _shutdown;
    }

    public async Task StopAsync()
    {
        if (_stopped || _host == null)
        {
            return;
        }

        using var deadline = Deadline();
        try
        {
            var stop = RequestStopAsync(deadline.Token);
            while (!stop.IsCompleted)
            {
                await WaitForProgressAsync(stop, _host.Services.GetRequiredService<UserNotifierJobs>().GetStatus(), deadline.Token, awaitCompletion: true);
            }

            await stop;
            _stopped = true;
        }
        catch (Exception exception)
        {
            _services.MarkUnusable(exception);
            throw new InvalidOperationException($"Notifier shutdown failed.\n{Logs}", exception);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        await StopAsync();
        _host?.Dispose();
        Logs.Dispose();
        await _database.DisposeAsync();

        Logs.EnsureNoErrors();
        External.EnsureNoUnexpectedRequests();
        DiscordApi.EnsureExpectationsMet();
        External.EnsureExpectationsMet();
        Logs.EnsureExpectationsMet();
    }
}
