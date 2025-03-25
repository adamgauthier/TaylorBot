﻿using Discord;
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

    ValueTask StartAsync();
    ValueTask StopAsync();

    SocketGuild ResolveRequiredGuild(SnowflakeId id);

    ValueTask<IUser> ResolveRequiredUserAsync(SnowflakeId id);
    ValueTask<IChannel> ResolveRequiredChannelAsync(SnowflakeId id);
    ValueTask<IGuildUser?> ResolveGuildUserAsync(IGuild guild, SnowflakeId userId);
    ValueTask<IGuildUser?> ResolveGuildUserAsync(SnowflakeId guildId, SnowflakeId userId);
}

public class TaylorBotClient : ITaylorBotClient
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
        _logger.LogTrace("Received interaction {Payload}", payload);

        var interaction = DeserializeInteraction(payload);

        await _interactionCreatedEvent.InvokeAsync(interaction);
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
            _logger.LogError(e, "Failed to deserialize interaction payload: {Payload}", payload);
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

        _logger.Log(_logSeverityToLogLevelMapper.MapFrom(log.Severity), "Discord.Net: {Message}", log.ToString(prependTimestamp: false));
        return Task.CompletedTask;
    }

    private Task ShardReadyAsync(DiscordSocketClient shardClient)
    {
        _logger.LogInformation("Shard #{ShardId} is ready! Serving {GuildCountText} out of {TotalGuildCount}. ShardCount {ShardCount}",
            shardClient.ShardId, "guild".ToQuantity(shardClient.Guilds.Count), DiscordShardedClient.Guilds.Count, DiscordShardedClient.Shards.Count);

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
            var restUser = await DiscordShardedClient.Rest.GetUserAsync(id.Id);
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
            var restChannel = await DiscordShardedClient.Rest.GetChannelAsync(id.Id);
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
            var restUser = await DiscordShardedClient.Rest.GetGuildUserAsync(guild.Id, userId.Id);
            return restUser;
        }

        return user;
    }
}
