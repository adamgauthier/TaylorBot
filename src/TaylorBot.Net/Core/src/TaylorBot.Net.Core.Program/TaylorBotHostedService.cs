using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Core.Program;

public partial class TaylorBotHostedService(IServiceProvider services, ILogger<TaylorBotHostedService> logger, TaskExceptionLogger taskExceptionLogger) : IHostedService
{
    private const GatewayIntents IntentMessageContent = (GatewayIntents)(1 << 15);

    private ITaylorBotClient? _client;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var registrars =
            GetBaseRegistrars()
            .Concat(GetMessageRegistrars())
            .Concat(GetReactionsRegistrars())
            .Concat(GetGuildsRegistrars())
            .Concat(GetGuildMembersRegistrars())
            .Concat(GetGuildBansRegistrars())
            .ToList();

        var intents = registrars.SelectMany(r => r.Intents).Distinct().ToList();
        var flaggedIntents = (GatewayIntents)(object)intents.Cast<int>().Aggregate(0, (acc, intent) => acc |= intent);

        services.GetRequiredService<DiscordSocketConfig>().GatewayIntents = flaggedIntents;

        _client = services.GetRequiredService<ITaylorBotClient>();

        foreach (var registrar in registrars)
        {
            registrar.RegisterEventHandler(_client);
        }

        // Wait to login in case of a boot loop
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        LogStartingClient(flaggedIntents, (int)flaggedIntents);

        await _client.StartAsync();
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Starting client with intents: {Intents} ({Value})")]
    public partial void LogStartingClient(GatewayIntents Intents, int Value);

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client != null)
        {
            await _client.StopAsync();
            LogClientsUnloaded();
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Clients unloaded!")]
    private partial void LogClientsUnloaded();

    private sealed class EventHandlerRegistrar(Action<ITaylorBotClient> register, GatewayIntents[]? intents = null)
    {
        public GatewayIntents[] Intents { get; } = intents ?? [];

        public void RegisterEventHandler(ITaylorBotClient taylorBotClient)
        {
            register(taylorBotClient);
        }
    }

    private IEnumerable<EventHandlerRegistrar> GetBaseRegistrars()
    {
        var shardReadyHandlers = services.GetServices<IShardReadyHandler>().ToList();
        if (shardReadyHandlers.Count > 0)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.ShardReady += async (socketClient) =>
                    await taskExceptionLogger.LogOnError(async () =>
                    {
                        foreach (var handler in shardReadyHandlers)
                        {
                            await handler.ShardReadyAsync(socketClient);
                        }
                    },
                        nameof(IShardReadyHandler)
                    );
            }, [GatewayIntents.Guilds, GatewayIntents.GuildMembers]);
        }

        var interactionHandler = services.GetService<IInteractionCreatedHandler>();
        if (interactionHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.InteractionCreated += async (interaction) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await interactionHandler.InteractionCreatedAsync(interaction),
                        nameof(IInteractionCreatedHandler)
                    );
            }, [GatewayIntents.Guilds, GatewayIntents.GuildMembers]);
        }
    }

    private IEnumerable<EventHandlerRegistrar> GetMessageRegistrars()
    {
        var messageReceivedHandler = services.GetService<IMessageReceivedHandler>();
        if (messageReceivedHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.MessageReceived += async (message) =>
                {
                    await taskExceptionLogger.LogOnError(async () =>
                        await messageReceivedHandler.MessageReceivedAsync(message),
                        nameof(IMessageReceivedHandler)
                    );
                };
            }, [IntentMessageContent, GatewayIntents.Guilds, GatewayIntents.GuildMessages, GatewayIntents.DirectMessages]);
        }

        var userMessageReceivedHandler = services.GetService<IUserMessageReceivedHandler>();
        if (userMessageReceivedHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.MessageReceived += async (message) =>
                {
                    if (message is SocketUserMessage userMessage)
                    {
                        await taskExceptionLogger.LogOnError(async () =>
                            await userMessageReceivedHandler.UserMessageReceivedAsync(userMessage),
                            nameof(IUserMessageReceivedHandler)
                        );
                    }
                };
            }, [IntentMessageContent, GatewayIntents.Guilds, GatewayIntents.GuildMessages, GatewayIntents.DirectMessages]);
        }

        var messageDeletedHandler = services.GetService<IMessageDeletedHandler>();
        if (messageDeletedHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.MessageDeleted += async (message, channel) =>
                {
                    await taskExceptionLogger.LogOnError(async () =>
                        await messageDeletedHandler.MessageDeletedAsync(message, channel),
                        nameof(IMessageDeletedHandler)
                    );
                };
            }, [IntentMessageContent, GatewayIntents.Guilds, GatewayIntents.GuildMessages, GatewayIntents.DirectMessages]);

        }

        var messageBulkDeletedHandler = services.GetService<IMessageBulkDeletedHandler>();
        if (messageBulkDeletedHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.MessagesBulkDeleted += async (messages, channel) =>
                {
                    await taskExceptionLogger.LogOnError(async () =>
                        await messageBulkDeletedHandler.MessageBulkDeletedAsync(messages, channel),
                        nameof(IMessageBulkDeletedHandler)
                    );
                };
            }, [IntentMessageContent, GatewayIntents.Guilds, GatewayIntents.GuildMessages, GatewayIntents.DirectMessages]);
        }

        var messageUpdatedHandler = services.GetService<IMessageUpdatedHandler>();
        if (messageUpdatedHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.MessageUpdated += async (oldMessage, newMessage, channel) =>
                {
                    await taskExceptionLogger.LogOnError(async () =>
                        await messageUpdatedHandler.MessageUpdatedAsync(oldMessage, newMessage, channel),
                        nameof(IMessageUpdatedHandler)
                    );
                };
            }, [IntentMessageContent, GatewayIntents.Guilds, GatewayIntents.GuildMessages, GatewayIntents.DirectMessages]);
        }
    }

    private IEnumerable<EventHandlerRegistrar> GetReactionsRegistrars()
    {
        var reactionAddedHandler = services.GetService<IReactionAddedHandler>();
        if (reactionAddedHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.ReactionAdded += async (message, channel, reaction) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await reactionAddedHandler.ReactionAddedAsync(message, channel, reaction),
                        nameof(IReactionAddedHandler)
                    );
            }, [GatewayIntents.GuildMessageReactions, GatewayIntents.DirectMessageReactions]);
        }

        var reactionRemovedHandler = services.GetService<IReactionRemovedHandler>();
        if (reactionRemovedHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.ReactionRemoved += async (message, channel, reaction) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await reactionRemovedHandler.ReactionRemovedAsync(message, channel, reaction),
                        nameof(IReactionRemovedHandler)
                    );
            }, [GatewayIntents.GuildMessageReactions, GatewayIntents.DirectMessageReactions]);
        }
    }

    private IEnumerable<EventHandlerRegistrar> GetGuildsRegistrars()
    {
        foreach (var joinedGuildHandler in services.GetServices<IJoinedGuildHandler>())
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.JoinedGuild += async (guild) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await joinedGuildHandler.JoinedGuildAsync(guild),
                        nameof(IJoinedGuildHandler)
                    );
            }, [GatewayIntents.Guilds]);
        }

        var guildUpdatedHandler = services.GetService<IGuildUpdatedHandler>();
        if (guildUpdatedHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.GuildUpdated += async (oldGuild, newGuild) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await guildUpdatedHandler.GuildUpdatedAsync(oldGuild, newGuild),
                        nameof(IGuildUpdatedHandler)
                    );
            }, [GatewayIntents.Guilds]);
        }

        var textChannelCreatedHandler = services.GetService<ITextChannelCreatedHandler>();
        if (textChannelCreatedHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.ChannelCreated += async (socketChannel) =>
                {
                    if (socketChannel is SocketTextChannel textChannel)
                    {
                        await taskExceptionLogger.LogOnError(async () =>
                            await textChannelCreatedHandler.TextChannelCreatedAsync(textChannel),
                            nameof(ITextChannelCreatedHandler)
                        );
                    }
                };
            }, [GatewayIntents.Guilds]);
        }
    }

    private IEnumerable<EventHandlerRegistrar> GetGuildMembersRegistrars()
    {
        var userUpdatedHandler = services.GetService<IUserUpdatedHandler>();
        if (userUpdatedHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.UserUpdated += async (oldUser, newUser) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await userUpdatedHandler.UserUpdatedAsync(oldUser, newUser),
                        nameof(IUserUpdatedHandler)
                    );
            }, [GatewayIntents.GuildMembers]);
        }

        var guildUserJoinedHandler = services.GetService<IGuildUserJoinedHandler>();
        if (guildUserJoinedHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.UserJoined += async (guildUser) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await guildUserJoinedHandler.GuildUserJoinedAsync(guildUser),
                        nameof(IGuildUserJoinedHandler)
                    );
            }, [GatewayIntents.GuildMembers]);
        }

        var guildUserLeftHandler = services.GetService<IGuildUserLeftHandler>();
        if (guildUserLeftHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.UserLeft += async (guild, user) =>
                {
                    await taskExceptionLogger.LogOnError(async () =>
                        await guildUserLeftHandler.GuildUserLeftAsync(guild, user),
                        nameof(IGuildUserLeftHandler)
                    );
                };
            }, [GatewayIntents.GuildMembers]);
        }
    }

    private IEnumerable<EventHandlerRegistrar> GetGuildBansRegistrars()
    {
        var guildUserBannedHandler = services.GetService<IGuildUserBannedHandler>();
        if (guildUserBannedHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.UserBanned += async (user, guild) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await guildUserBannedHandler.GuildUserBannedAsync(user, guild),
                        nameof(IGuildUserBannedHandler)
                    );
            }, [GatewayIntents.GuildBans]);
        }

        var guildUserUnbannedHandler = services.GetService<IGuildUserUnbannedHandler>();
        if (guildUserUnbannedHandler != null)
        {
            yield return new EventHandlerRegistrar((client) =>
            {
                client.DiscordShardedClient.UserUnbanned += async (user, guild) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await guildUserUnbannedHandler.GuildUserUnbannedAsync(user, guild),
                        nameof(IGuildUserUnbannedHandler)
                    );
            }, [GatewayIntents.GuildBans]);
        }
    }
}
