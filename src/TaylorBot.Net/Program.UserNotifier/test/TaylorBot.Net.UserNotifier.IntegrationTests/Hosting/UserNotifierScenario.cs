using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using TaylorBot.Net.Core.Tasks;
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
        UserNotifierJob? job = null, IReadOnlyDictionary<string, string?>? settings = null)
    {
        UserNotifierScenario scenario = new(services, await services.CreateDatabaseAsync(), cancellationToken);

        try
        {
            await scenario.StartAsync(job, settings);
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

    private async Task StartAsync(UserNotifierJob? job, IReadOnlyDictionary<string, string?>? settings)
    {
        var configuration = ScenarioConfiguration.Create(_services, new NpgsqlConnectionStringBuilder(_database.ConnectionString), job);
        if (settings != null)
        {
            foreach (var setting in settings)
            {
                configuration[setting.Key] = setting.Value;
            }
        }

        _host = UserNotifierProgram.CreateHostBuilder(new HostBuilder()
            .ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(configuration))
            .ConfigureLogging(builder => builder.AddProvider(Logs).SetMinimumLevel(LogLevel.Debug)))
            .ConfigureServices(services =>
            {
                services.Replace(ServiceDescriptor.Singleton<TimeProvider>(_time));
                _gateway.Configure(services);
                NotifierTransports.Configure(services, External);
            })
            .Build();

        using var deadline = Deadline();
        await _host.StartAsync(deadline.Token);
        await _gateway.WaitUntilReadyAsync(deadline.Token);
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
            if (drained && status.Jobs.Count == Enum.GetValues<UserNotifierJob>().Length &&
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
            _time.Advance(timers.DueTimes.Min() - _time.GetUtcNow());
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
        _time.Advance(elapsed);
        await SettleAsync(deadline.Token);
        return new([.. DiscordApi.Requests.Skip(start)]);
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
        _time.Advance(before.NextRunAt!.Value - Now);

        using var deadline = Deadline();
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
        _time.Advance(due - Now);
        using var deadline = Deadline();
        await gate.WaitForRequestAsync(deadline.Token);
    }

    public async Task AdvanceWhileRequestBlockedAsync(UserNotifierJob job, TimeSpan elapsed)
    {
        _time.Advance(elapsed);
        var jobs = _host!.Services.GetRequiredService<UserNotifierJobs>();
        using var deadline = Deadline();

        while (true)
        {
            var status = jobs.GetStatus();
            if (status.Jobs.Where(status => status.Name != job).All(status => !status.IsRunning && status.NextRunAt > Now))
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
            _shutdown = _host?.StopAsync(cancellationToken) ?? Task.CompletedTask;
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
