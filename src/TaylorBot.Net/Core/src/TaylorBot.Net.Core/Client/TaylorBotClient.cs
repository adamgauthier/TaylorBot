using Discord;
using Discord.WebSocket;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaylorBot.Net.Core.Events;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.Client;

public interface ITaylorBotClient
{
    event Func<Interaction, Task> InteractionCreated;

    DiscordShardedClient DiscordShardedClient { get; }
    DiscordSocketRestClient RestClient { get; }

    ValueTask StartAsync();
    ValueTask StopAsync();

    SocketGuild ResolveRequiredGuild(SnowflakeId id);

    ValueTask<IUser> ResolveRequiredUserAsync(SnowflakeId id);
    ValueTask<IChannel> ResolveRequiredChannelAsync(SnowflakeId id);
    ValueTask<IGuildUser?> ResolveGuildUserAsync(IGuild guild, SnowflakeId userId);
    ValueTask<IGuildUser?> ResolveGuildUserAsync(SnowflakeId guildId, SnowflakeId userId);
}

public partial class TaylorBotClient : ITaylorBotClient
{
    private readonly ILogger<TaylorBotClient> _logger;
    private readonly ILogSeverityToLogLevelMapper _logSeverityToLogLevelMapper;
    private readonly TaylorBotToken _taylorBotToken;
    private readonly RawEventsHandler _rawEventsHandler;

    private readonly AsyncEvent<Func<Interaction, Task>> _interactionCreatedEvent = new();
    public event Func<Interaction, Task> InteractionCreated
    {
        add { _interactionCreatedEvent.Add(value); }
        remove { _interactionCreatedEvent.Remove(value); }
    }

    public DiscordShardedClient DiscordShardedClient { get; }

    private readonly Lazy<DiscordSocketRestClient> _restClient;
    public DiscordSocketRestClient RestClient => _restClient.Value;

    public TaylorBotClient(
        ILogger<TaylorBotClient> logger,
        ILogSeverityToLogLevelMapper logSeverityToLogLevelMapper,
        TaylorBotToken taylorBotToken,
        RawEventsHandler rawEventsHandler,
        DiscordShardedClient discordShardedClient
    )
    {
        _logger = logger;
        _logSeverityToLogLevelMapper = logSeverityToLogLevelMapper;
        _taylorBotToken = taylorBotToken;
        _rawEventsHandler = rawEventsHandler;
        DiscordShardedClient = discordShardedClient;
        _restClient = new(GetRestClient);

        DiscordShardedClient.Log += LogAsync;
        DiscordShardedClient.ShardReady += ShardReadyAsync;
    }

    public async ValueTask StartAsync()
    {
        await DiscordShardedClient.LoginAsync(TokenType.Bot, _taylorBotToken.Token);

        foreach (var shard in DiscordShardedClient.Shards)
        {
            _rawEventsHandler.HandleRawEvent(shard, "INTERACTION_CREATE", HandleInteractionAsync);
        }

        await DiscordShardedClient.StartAsync();
    }

    private async Task HandleInteractionAsync(string payload)
    {
        LogReceivedInteraction(payload);

        var interaction = DeserializeInteraction(payload);

        await _interactionCreatedEvent.InvokeAsync(interaction);
    }

    private DiscordSocketRestClient GetRestClient()
    {
        var shards = DiscordShardedClient.Shards;
        DiscordSocketRestClient? restClient = null;

        foreach (var shard in shards)
        {
            if (shard != null)
            {
                restClient = shard.Rest;
                // Indicates that the shard went through READY event and is initialized
                if (restClient.CurrentUser != null)
                {
                    return restClient;
                }
            }
        }

        ArgumentNullException.ThrowIfNull(restClient);
        return restClient;
    }

    private Interaction DeserializeInteraction(string payload)
    {
        try
        {
            var interaction = JsonSerializer.Deserialize<Interaction>(payload);
            ArgumentNullException.ThrowIfNull(interaction);
            return interaction;
        }
        catch (Exception e)
        {
            LogFailedToDeserializeInteraction(e, payload);
            throw;
        }
    }

    public async ValueTask StopAsync()
    {
        await DiscordShardedClient.StopAsync();
    }

    private Task LogAsync(LogMessage log)
    {
        if (_rawEventsHandler.Callbacks.Keys.Any(rawEvent => log.Message == $"Unknown Dispatch ({rawEvent})"))
        {
            return Task.CompletedTask;
        }

        LogDiscordNetMessage(_logSeverityToLogLevelMapper.MapFrom(log.Severity), log.ToString(prependTimestamp: false));
        return Task.CompletedTask;
    }

    private Task ShardReadyAsync(DiscordSocketClient shardClient)
    {
        LogShardReady(shardClient.ShardId, "guild".ToQuantity(shardClient.Guilds.Count), DiscordShardedClient.Guilds.Count, DiscordShardedClient.Shards.Count);

        return Task.CompletedTask;
    }

    public SocketGuild ResolveRequiredGuild(SnowflakeId id)
    {
        return DiscordShardedClient.GetGuild(id.Id) ?? throw new ArgumentException($"Could not resolve Guild ID {id}.");
    }

    public async ValueTask<IUser> ResolveRequiredUserAsync(SnowflakeId id)
    {
        var user = await ((IDiscordClient)DiscordShardedClient).GetUserAsync(id.Id);
        if (user == null)
        {
            var restUser = await RestClient.GetUserAsync(id.Id);
            if (restUser != null)
            {
                return restUser;
            }

            throw new ArgumentException($"Could not resolve user with id '{id}'.");
        }

        return user;
    }

    public async ValueTask<IChannel> ResolveRequiredChannelAsync(SnowflakeId id)
    {
        var channel = await ((IDiscordClient)DiscordShardedClient).GetChannelAsync(id.Id);

        if (channel == null)
        {
            var restChannel = await RestClient.GetChannelAsync(id.Id);
            if (restChannel != null)
            {
                return restChannel;
            }

            throw new ArgumentException($"Could not resolve channel with id '{id}'.");
        }

        return channel;
    }

    public async ValueTask<IGuildUser?> ResolveGuildUserAsync(SnowflakeId guildId, SnowflakeId userId)
    {
        var guild = DiscordShardedClient.GetGuild(guildId);
        if (guild == null)
        {
            return null;
        }
        return await ResolveGuildUserAsync(guild, userId);
    }

    public async ValueTask<IGuildUser?> ResolveGuildUserAsync(IGuild guild, SnowflakeId userId)
    {
        var user = await guild.GetUserAsync(userId.Id).ConfigureAwait(false);

        if (user == null)
        {
            var restUser = await RestClient.GetGuildUserAsync(guild.Id, userId.Id);
            return restUser;
        }

        return user;
    }

    [LoggerMessage(Level = LogLevel.Trace, Message = "Received interaction {Payload}")]
    private partial void LogReceivedInteraction(string payload);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to deserialize interaction payload: {Payload}")]
    private partial void LogFailedToDeserializeInteraction(Exception exception, string payload);

    [LoggerMessage(Message = "Discord.Net: {Message}")]
    private partial void LogDiscordNetMessage(LogLevel logLevel, string message);

    [LoggerMessage(Level = LogLevel.Information, Message = "Shard #{ShardId} is ready! Serving {GuildCountText} out of {TotalGuildCount}. ShardCount {ShardCount}")]
    private partial void LogShardReady(int shardId, string guildCountText, int totalGuildCount, int shardCount);
}
