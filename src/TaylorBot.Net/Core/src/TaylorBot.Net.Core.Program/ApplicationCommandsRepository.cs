using Discord;
using Discord.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Core.Program;

public sealed partial class ApplicationCommandsRepository(
    BackgroundTasks backgroundTasks,
    IMemoryCache memoryCache,
    Lazy<ITaylorBotClient> taylorBotClient,
    TimeProvider timeProvider,
    ILogger<ApplicationCommandsRepository> logger) : IApplicationCommandsRepository
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromDays(1);
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan LookupTimeout = TimeSpan.FromSeconds(2);
    private readonly Lock _lock = new();

    private sealed class CommandCache
    {
        public Dictionary<string, SnowflakeId>? Commands { get; set; }
        public DateTimeOffset LoadedAt { get; set; }
        public DateTimeOffset RefreshAfter { get; set; }
        public Task Refresh { get; set; } = Task.CompletedTask;
    }

    public ValueTask<SnowflakeId?> GetCommandIdAsync(string name, bool waitForRefresh) => GetCommandIdAsync(name, guildId: null, waitForRefresh);

    public ValueTask<SnowflakeId?> GetGuildCommandIdAsync(SnowflakeId guildId, string name, bool waitForRefresh) => GetCommandIdAsync(name, guildId, waitForRefresh);

    public Task CacheCommandsAsync() => WaitForRefreshAsync(RefreshIfNeeded(GetCache(guildId: null), name: null, guildId: null), guildId: null);

    private CommandCache GetCache(SnowflakeId? guildId)
    {
        lock (_lock)
        {
            return memoryCache.GetOrCreate($"application-command-cache:{Scope(guildId)}", entry =>
            {
                entry.SlidingExpiration = CacheLifetime;
                return new CommandCache();
            })!;
        }
    }

    private async ValueTask<SnowflakeId?> GetCommandIdAsync(string name, SnowflakeId? guildId, bool waitForRefresh)
    {
        var cache = GetCache(guildId);
        SnowflakeId? cachedId;
        lock (_lock)
        {
            cachedId = cache.Commands?.GetValueOrDefault(name);
        }

        var refresh = RefreshIfNeeded(cache, name, guildId);
        if (cachedId != null)
        {
            return cachedId;
        }
        if (waitForRefresh)
        {
            await WaitForRefreshAsync(refresh, guildId);
        }
        return ReadCommandId(cache, name, guildId);
    }

    private SnowflakeId? ReadCommandId(CommandCache cache, string name, SnowflakeId? guildId)
    {
        lock (_lock)
        {
            if (cache.Commands?.TryGetValue(name, out var id) == true)
            {
                return id;
            }
        }

        LogUnresolvedCommand(name, Scope(guildId));
        return null;
    }

    private Task RefreshIfNeeded(CommandCache cache, string? name, SnowflakeId? guildId)
    {
        lock (_lock)
        {
            var now = timeProvider.GetUtcNow();
            if (cache.Commands != null && now - cache.LoadedAt < CacheLifetime && (name == null || cache.Commands.ContainsKey(name)))
            {
                return Task.CompletedTask;
            }

            if (!cache.Refresh.IsCompleted)
            {
                return cache.Refresh;
            }

            if (now < cache.RefreshAfter)
            {
                return Task.CompletedTask;
            }

            cache.RefreshAfter = now + RefreshInterval;
            cache.Refresh = backgroundTasks.Queue(() => RefreshAsync(cache, guildId), nameof(ApplicationCommandsRepository));
            return cache.Refresh;
        }
    }

    private async Task WaitForRefreshAsync(Task refresh, SnowflakeId? guildId)
    {
        try
        {
            await refresh.WaitAsync(LookupTimeout);
        }
        catch (TimeoutException exception)
        {
            LogRefreshFailed(exception, Scope(guildId));
        }
    }

    private async Task RefreshAsync(CommandCache cache, SnowflakeId? guildId)
    {
        // Network deadlines use wall-clock time, independently of application scheduling.
        using CancellationTokenSource timeout = new(LookupTimeout);
        RequestOptions options = new() { CancelToken = timeout.Token, Timeout = (int)LookupTimeout.TotalMilliseconds, RetryMode = RetryMode.AlwaysFail };
        try
        {
            IReadOnlyCollection<IApplicationCommand> commands = guildId == null
                ? await taylorBotClient.Value.RestClient.GetGlobalApplicationCommands(options: options)
                : await taylorBotClient.Value.ResolveRequiredGuild(guildId).GetApplicationCommandsAsync(options: options);
            var lookup = commands.ToDictionary(command => command.Name, command => new SnowflakeId(command.Id));
            lock (_lock)
            {
                if (cache.Commands == null)
                {
                    // Initial population does not consume the first miss-triggered refresh.
                    cache.RefreshAfter = default;
                }
                cache.Commands = lookup;
                cache.LoadedAt = timeProvider.GetUtcNow();
            }
        }
        catch (Exception exception) when (exception is HttpException or HttpRequestException or OperationCanceledException or TimeoutException or RateLimitedException or JsonException)
        {
            LogRefreshFailed(exception, Scope(guildId));
        }
    }

    private static string Scope(SnowflakeId? guildId) => guildId == null ? "global" : $"guild:{guildId}";

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not resolve command {CommandName} in {CommandScope} for a slash-command mention.")]
    private partial void LogUnresolvedCommand(string commandName, string commandScope);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not refresh {CommandScope} slash-command mentions; keeping cached IDs and using plain text for unresolved commands.")]
    private partial void LogRefreshFailed(Exception exception, string commandScope);
}
