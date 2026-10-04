using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord.DiscordNet;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;
using TaylorBot.Net.Core.Random;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;

public sealed class CommandsDiscordScenario : IAsyncDisposable
{
    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(15);
    private readonly ScenarioDatabase _database;
    private readonly DataServices _dataServices;
    private readonly ScenarioLogs _logs = new();
    private readonly DiscordApi _api = new();
    private readonly DiscordNetSession _session;
    private IHost? _host;
    private bool _disposed;
    private bool _failureReported;
    private readonly CancellationToken _cancellationToken;

    public ScenarioData Given { get; }
    public ScenarioState State { get; }
    public DiscordDriver Discord { get; }
    public DiscordApi DiscordApi => _api;
    public ExternalApi External { get; } = new();

    private CommandsDiscordScenario(DataServices dataServices, ScenarioDatabase database, CancellationToken cancellationToken)
    {
        _dataServices = dataServices;
        _database = database;
        _cancellationToken = cancellationToken;
        _session = new(_api);

        Given = new(database, DispatchAsync, _api);
        State = new(database);
        Discord = new(_api, DispatchAsync);
    }

    public static async Task<CommandsDiscordScenario> CreateAsync(DataServices data, CancellationToken cancellationToken, int dailyBonusInterval = 5,
        IReadOnlyDictionary<string, string?>? settings = null)
    {
        CommandsDiscordScenario scenario = new(data, await data.CreateDatabaseAsync(), cancellationToken);

        try
        {
            await scenario.StartAsync(data, dailyBonusInterval, settings, cancellationToken);
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
                throw new AggregateException("Application startup and cleanup failed.", startupFailure, cleanupFailure);
            }

            throw;
        }
    }

    private async Task StartAsync(DataServices data, int dailyBonusInterval, IReadOnlyDictionary<string, string?>? settings, CancellationToken cancellationToken)
    {
        NpgsqlConnectionStringBuilder connection = new(_database.ConnectionString);
        Dictionary<string, string?> configuration = new()
        {
            ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = "no_application_insights",
            ["Discord:Token"] = "MTAwMDAwMDAwMDAwMDAwMDAx.synthetic.synthetic-test-token-never-a-real-credential",
            ["Discord:ShardCount"] = "1",
            ["Discord:StartupDelay"] = "00:00:00",
            ["CommandClient:UseRedisCache"] = "true",
            ["CommandApplication:DailyLimits:custom-search:MaxUsesForUser"] = "7",
            ["EntityTracker:UseRedisCache"] = "true",
            ["DatabaseConnection:Host"] = connection.Host,
            ["DatabaseConnection:Port"] = $"{connection.Port}",
            ["DatabaseConnection:Username"] = connection.Username,
            ["DatabaseConnection:Password"] = connection.Password,
            ["DatabaseConnection:Database"] = connection.Database,
            ["DatabaseConnection:ApplicationName"] = "IntegrationTests",
            ["DatabaseConnection:MaxPoolSize"] = "10",
            ["DatabaseConnection:GssEncryptionMode"] = nameof(GssEncryptionMode.Disable),
            ["RedisConnection:Host"] = data.RedisHost,
            ["RedisConnection:Port"] = $"{data.RedisPort}",
            ["RedisConnection:Password"] = "",
            ["TaypointWill:DaysOfInactivityBeforeWillCanBeClaimed"] = "20",
            ["DailyPayout:DailyPayoutAmount"] = "100",
            ["DailyPayout:DaysForBonus"] = $"{dailyBonusInterval}",
            ["CommandApplication:DailyLimits:weather-report:MaxUsesForUser"] = "7",
            ["LastFm:LastFmEmbedFooterIconUrl"] = "https://example.invalid/lastfm.png",
            ["LastFm:LastFmApiKey"] = "synthetic",
            ["LastFm:LastFmApiSecret"] = "synthetic",
            ["Google:GoogleApiKey"] = "synthetic",
            ["Image:GoogleCustomSearchEngineId"] = "synthetic",
            ["SerpApi:ApiKey"] = "synthetic",
            ["ModMail:ReceivedLogEmbedFooterIconUrl"] = "https://example.invalid/icon.png",
            ["Weather:PirateWeatherApiKey"] = "synthetic",
            ["WolframAlpha:AppId"] = "synthetic",
            ["Imgur:ClientId"] = "synthetic",
            ["Signature:StorageAccountUri"] = "https://storage.invalid",
            ["Heist:TimeSpanBeforeHeistStarts"] = "00:00:01",
        };

        foreach (var action in new[]
        {
            "google-places-search", "youtube-search", "youtube-search-legacy", "horoscope", "urbandictionary-search",
            "wolframalpha-query", "imgur-upload", "submit-signature", "redeem-coupon", "heist", "rps", "roll", "generate-recap",
        })
        {
            configuration[$"CommandApplication:DailyLimits:{action}:MaxUsesForUser"] = "20";
            configuration[$"CommandApplication:DailyLimits:{action}:FriendlyName"] = action;
        }

        if (settings != null)
        {
            foreach (var setting in settings)
            {
                configuration[setting.Key] = setting.Value;
            }
        }

        _host = DiscordCommandsProgram.CreateHostBuilder(new HostBuilder()
            .ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(configuration))
            .ConfigureLogging(builder => builder.AddProvider(_logs).SetMinimumLevel(LogLevel.Debug)))
            .ConfigureServices(services =>
            {
                var scheduledProducer = services.Single(descriptor => descriptor.ImplementationType == typeof(ValentineGiveawayReadyHandler));
                services.Remove(scheduledProducer);
                _session.Configure(services);
                ExternalTransports.Configure(services, _api, External);
                services.AddSingleton<IPseudoRandom>(new SeededRandom());
            })
            .Build();

        using var startup = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        startup.CancelAfter(OperationTimeout);

        await _host.StartAsync(startup.Token);
        await _session.WaitUntilReadyAsync(startup.Token);
        await DrainAsync(startup.Token);

        _session.ObservePrefixCommands(_host.Services);
    }

    private async Task DispatchAsync(string eventName, object data)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        deadline.CancelAfter(OperationTimeout);

        await _session.DispatchAsync(eventName, data, deadline.Token);
        await DrainAsync(deadline.Token);

        if (eventName == "MESSAGE_CREATE")
        {
            var message = System.Text.Json.JsonSerializer.SerializeToElement(data);
            if (_api.ForMessage(message.GetProperty("channel_id").GetString()!, message.GetProperty("id").GetString()!).Count == 0)
            {
                throw new InvalidOperationException($"Prefix command completed without a response:\n{_logs}");
            }
        }
    }

    private async Task DrainAsync(CancellationToken cancellationToken)
    {
        var host = _host ?? throw new InvalidOperationException("Application has not started.");

        try
        {
            await host.Services.GetRequiredService<BackgroundTasks>().DrainAsync(cancellationToken);
            _api.EnsureNoUnexpectedRequests();
            External.EnsureNoUnexpectedRequests();
            _logs.EnsureNoErrors();
        }
        catch (Exception exception)
        {
            _failureReported = true;
            throw new InvalidOperationException($"Application scenario failed:\n{_logs}", exception);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            if (_host != null)
            {
                using CancellationTokenSource shutdown = new(OperationTimeout);
                await _session.DrainPrefixCommandsAsync(shutdown.Token);
                await _host.StopAsync(shutdown.Token);
            }
        }
        catch (Exception exception)
        {
            _dataServices.MarkUnusable(exception);
            throw;
        }
        finally
        {
            _host?.Dispose();
            _logs.Dispose();
        }

        await _database.DisposeAsync();

        if (!_failureReported)
        {
            _api.EnsureExpectationsMet();
            External.EnsureExpectationsMet();
            _logs.EnsureNoErrors();
        }
    }

    private sealed class SeededRandom : IPseudoRandom
    {
        private readonly Random _random = new(Seed: 42);

        public int GetInt32Exclusive(int fromInclusive, int toExclusive) => _random.Next(fromInclusive, toExclusive);
    }
}
