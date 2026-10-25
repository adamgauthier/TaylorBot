using System.Diagnostics;
using System.Net.Http.Json;
using Discord;
using Discord.Net.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.UserNotifier.Program;

namespace TaylorBot.Net.UserNotifier.LoadTests;

internal static class Program
{
    public static async Task<int> Main()
    {
        using HttpClient controller = new()
        {
            BaseAddress = new("http://load-controller:8080"),
            Timeout = TimeSpan.FromSeconds(30),
        };
        var settings = await controller.GetFromJsonAsync<Dictionary<string, string?>>("configuration")
            ?? throw new InvalidOperationException("Controller returned no worker configuration.");
        using ErrorCounter errors = new();
        var builder = UserNotifierProgram.AddApplicationConfiguration(Host.CreateDefaultBuilder())
            .ConfigureAppConfiguration(config => config.AddInMemoryCollection(settings))
            .ConfigureLogging(logging => logging.AddProvider(errors));
        using var host = UserNotifierProgram.CreateHostBuilder(builder)
            .ConfigureServices(services =>
            {
                services.PostConfigure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(60));
                // Keep Discord.Net's real HTTP and WebSocket transports; only their destination is synthetic.
                services.AddSingleton(provider =>
                {
                    var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<Core.Program.Options.DiscordOptions>>().CurrentValue;
                    return new DiscordSocketConfig
                    {
                        GatewayIntents = GatewayIntents.None,
                        TotalShards = (int?)options.ShardCount,
                        MessageCacheSize = (int)(options.MessageCacheSize ?? 0),
                        RestClientProvider = _ => DefaultRestClientProvider.Instance("http://load-controller:8080/api/"),
                    };
                });
            }).Build();

        return await RunAsync(host, controller, errors);
    }

    private static async Task<int> RunAsync(IHost host, HttpClient controller, ErrorCounter errors)
    {
        var run = UserNotifierProgram.RunAsync(host);
        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        try
        {
            return await MonitorAsync(host, controller, errors, run, lifetime);
        }
        finally
        {
            lifetime.StopApplication();
            await run;
        }
    }

    private static async Task<int> MonitorAsync(IHost host, HttpClient controller, ErrorCounter errors, Task<int> run,
        IHostApplicationLifetime lifetime)
    {
        while (!lifetime.ApplicationStarted.IsCancellationRequested && !run.IsCompleted)
        {
            await Task.WhenAny(run, Task.Delay(TimeSpan.FromMilliseconds(100)));
        }
        if (run.IsCompleted)
        {
            return await run;
        }

        var client = host.Services.GetRequiredService<ITaylorBotClient>().DiscordShardedClient;
        long received = 0;
        long ready = 0;
        client.MessageReceived += _ =>
        {
            Interlocked.Increment(ref received);
            return Task.CompletedTask;
        };
        client.ShardReady += _ =>
        {
            Interlocked.Increment(ref ready);
            return Task.CompletedTask;
        };
        var background = host.Services.GetRequiredService<BackgroundTasks>();
        var health = host.Services.GetRequiredService<DiscordGatewayHealthService>();
        using var process = Process.GetCurrentProcess();
        while (!run.IsCompleted && !lifetime.ApplicationStopping.IsCancellationRequested)
        {
            var samplingStarted = Stopwatch.GetTimestamp();
            var samplingAllocated = GC.GetAllocatedBytesForCurrentThread();
            process.Refresh();
            var memory = GC.GetGCMemoryInfo();
            var shards = client.Shards;
            var guilds = shards.SelectMany(shard => shard.Guilds);
            long members = 0, channels = 0, messages = 0;
            var count = 0;
            foreach (var guild in guilds)
            {
                count++;
                members += guild.Users.Count;
                channels += guild.Channels.Count;
                foreach (var channel in guild.TextChannels)
                {
                    messages += channel.CachedMessages.Count;
                }
            }
            WorkerSample sample = new(DateTimeOffset.UtcNow, process.WorkingSet64, process.PrivateMemorySize64,
                GC.GetTotalMemory(forceFullCollection: false), memory.TotalCommittedBytes, memory.FragmentedBytes,
                GC.GetTotalAllocatedBytes(precise: false), memory.TotalAvailableMemoryBytes, GC.CollectionCount(generation: 0), GC.CollectionCount(generation: 1),
                GC.CollectionCount(generation: 2), GC.GetTotalPauseDuration().Ticks, background.PendingCount,
                health.GetSnapshot().Shards.Count(shard => shard.Healthy),
                count, members, channels, messages, Interlocked.Read(ref received), Interlocked.Read(ref ready), errors.Count,
                ReadCgroup("memory.current"), ReadCgroup("memory.peak"), ReadCgroup("memory.events"),
                ReadCgroup("cpu.stat"),
                System.Runtime.GCSettings.IsServerGC,
                ThreadPool.ThreadCount, ThreadPool.PendingWorkItemCount, ThreadPool.CompletedWorkItemCount,
                Stopwatch.GetElapsedTime(samplingStarted).TotalMilliseconds, GC.GetAllocatedBytesForCurrentThread() - samplingAllocated);
            using var response = await controller.PostAsJsonAsync("samples", sample);
            response.EnsureSuccessStatusCode();
            var status = await response.Content.ReadFromJsonAsync<LoadStatus>()
                ?? throw new InvalidOperationException("Controller returned no load status.");
            if (status.Finished)
            {
                lifetime.StopApplication();
                var exit = await run;
                return status.Failure != null || errors.Count != 0 ? 2 : exit;
            }
            await Task.WhenAny(run, Task.Delay(TimeSpan.FromSeconds(5)));
        }
        return await run;
    }

    private static string? ReadCgroup(string filename)
    {
        var path = Path.Combine("/sys/fs/cgroup", filename);
        return File.Exists(path) ? File.ReadAllText(path).Trim() : null;
    }

    private sealed class ErrorCounter : ILoggerProvider
    {
        private long _count;
        public long Count => Interlocked.Read(ref _count);
        public ILogger CreateLogger(string categoryName) => new Counter(this);
        public void Dispose() { }

        private sealed class Counter(ErrorCounter owner) : ILogger
        {
            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (IsEnabled(logLevel))
                {
                    Interlocked.Increment(ref owner._count);
                }
            }
        }
    }
}
