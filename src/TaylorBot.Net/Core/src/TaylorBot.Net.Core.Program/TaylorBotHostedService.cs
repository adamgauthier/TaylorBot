using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Core.Program
{
    public class TaylorBotHostedService : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<TaylorBotHostedService> _logger;
        private readonly TaskExceptionLogger _taskExceptionLogger;
        private ITaylorBotClient? _client;

        public TaylorBotHostedService(IServiceProvider services)
        {
            _services = services;
            _logger = services.GetRequiredService<ILogger<TaylorBotHostedService>>();
            _taskExceptionLogger = services.GetRequiredService<TaskExceptionLogger>();
        }

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

            _services.GetRequiredService<DiscordSocketConfig>().GatewayIntents = flaggedIntents;

            _client = _services.GetRequiredService<ITaylorBotClient>();

            foreach (var registrar in registrars)
            {
                registrar.RegisterEventHandler(_client);
            }

            // Wait to login in case of a boot loop
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await _client.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_client != null)
            {
                await _client.StopAsync();
                _logger.LogInformation(LogString.From("Clients unloaded!"));
            }
        }

        private class EventHandlerRegistrar
        {
            private readonly Action<ITaylorBotClient> _register;

            public GatewayIntents[] Intents { get; }

            public EventHandlerRegistrar(Action<ITaylorBotClient> register, GatewayIntents[]? intents = null)
            {
                _register = register;
                Intents = intents ?? Array.Empty<GatewayIntents>();
            }

            public void RegisterEventHandler(ITaylorBotClient taylorBotClient)
            {
                _register(taylorBotClient);
            }
        }

        private IEnumerable<EventHandlerRegistrar> GetBaseRegistrars()
        {
            var shardReadyHandler = _services.GetService<IShardReadyHandler>();
            if (shardReadyHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.ShardReady += async (socketClient) =>
                        await _taskExceptionLogger.LogOnError(async () =>
                            await shardReadyHandler.ShardReadyAsync(socketClient),
                            nameof(IShardReadyHandler)
                        );
                });
            }

            var allReadyHandler = _services.GetService<IAllReadyHandler>();
            if (allReadyHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.AllShardsReady += async () =>
                        await _taskExceptionLogger.LogOnError(async () =>
                            await allReadyHandler.AllShardsReadyAsync(),
                            nameof(IAllReadyHandler)
                        );
                });
            }
        }

        private IEnumerable<EventHandlerRegistrar> GetMessageRegistrars()
        {
            var userMessageReceivedHandler = _services.GetService<IUserMessageReceivedHandler>();
            if (userMessageReceivedHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.MessageReceived += async (message) =>
                    {
                        if (message is SocketUserMessage userMessage)
                        {
                            await _taskExceptionLogger.LogOnError(async () =>
                                await userMessageReceivedHandler.UserMessageReceivedAsync(userMessage),
                                nameof(IUserMessageReceivedHandler)
                            );
                        }
                    };
                }, new[] { GatewayIntents.GuildMessages, GatewayIntents.DirectMessages });
            }

            var messageDeletedHandler = _services.GetService<IMessageDeletedHandler>();
            if (messageDeletedHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.MessageDeleted += async (message, channel) =>
                    {
                        await _taskExceptionLogger.LogOnError(async () =>
                            await messageDeletedHandler.MessageDeletedAsync(message, channel),
                            nameof(IMessageDeletedHandler)
                        );
                    };
                }, new[] { GatewayIntents.GuildMessages, GatewayIntents.DirectMessages });

            }

            var messageBulkDeletedHandler = _services.GetService<IMessageBulkDeletedHandler>();
            if (messageBulkDeletedHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.MessagesBulkDeleted += async (messages, channel) =>
                    {
                        await _taskExceptionLogger.LogOnError(async () =>
                            await messageBulkDeletedHandler.MessageBulkDeletedAsync(messages, channel),
                            nameof(IMessageBulkDeletedHandler)
                        );
                    };
                }, new[] { GatewayIntents.GuildMessages, GatewayIntents.DirectMessages });
            }
        }

        private IEnumerable<EventHandlerRegistrar> GetReactionsRegistrars()
        {
            var reactionAddedHandler = _services.GetService<IReactionAddedHandler>();
            if (reactionAddedHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.ReactionAdded += async (message, channel, reaction) =>
                        await _taskExceptionLogger.LogOnError(async () =>
                            await reactionAddedHandler.ReactionAddedAsync(message, channel, reaction),
                            nameof(IReactionAddedHandler)
                        );
                }, new[] { GatewayIntents.GuildMessageReactions, GatewayIntents.DirectMessageReactions });
            }

            var reactionRemovedHandler = _services.GetService<IReactionRemovedHandler>();
            if (reactionRemovedHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.ReactionRemoved += async (message, channel, reaction) =>
                        await _taskExceptionLogger.LogOnError(async () =>
                            await reactionRemovedHandler.ReactionRemovedAsync(message, channel, reaction),
                            nameof(IReactionRemovedHandler)
                        );
                }, new[] { GatewayIntents.GuildMessageReactions, GatewayIntents.DirectMessageReactions });
            }
        }

        private IEnumerable<EventHandlerRegistrar> GetGuildsRegistrars()
        {
            foreach (var joinedGuildHandler in _services.GetServices<IJoinedGuildHandler>())
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.JoinedGuild += async (guild) =>
                        await _taskExceptionLogger.LogOnError(async () =>
                            await joinedGuildHandler.JoinedGuildAsync(guild),
                            nameof(IJoinedGuildHandler)
                        );
                }, new[] { GatewayIntents.Guilds });
            }

            var guildUpdatedHandler = _services.GetService<IGuildUpdatedHandler>();
            if (guildUpdatedHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.GuildUpdated += async (oldGuild, newGuild) =>
                        await _taskExceptionLogger.LogOnError(async () =>
                            await guildUpdatedHandler.GuildUpdatedAsync(oldGuild, newGuild),
                            nameof(IGuildUpdatedHandler)
                        );
                }, new[] { GatewayIntents.Guilds });
            }

            var textChannelCreatedHandler = _services.GetService<ITextChannelCreatedHandler>();
            if (textChannelCreatedHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.ChannelCreated += async (socketChannel) =>
                    {
                        if (socketChannel is SocketTextChannel textChannel)
                        {
                            await _taskExceptionLogger.LogOnError(async () =>
                                await textChannelCreatedHandler.TextChannelCreatedAsync(textChannel),
                                nameof(ITextChannelCreatedHandler)
                            );
                        }
                    };
                }, new[] { GatewayIntents.Guilds });
            }
        }

        private IEnumerable<EventHandlerRegistrar> GetGuildMembersRegistrars()
        {
            var userUpdatedHandler = _services.GetService<IUserUpdatedHandler>();
            if (userUpdatedHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.UserUpdated += async (oldUser, newUser) =>
                        await _taskExceptionLogger.LogOnError(async () =>
                            await userUpdatedHandler.UserUpdatedAsync(oldUser, newUser),
                            nameof(IUserUpdatedHandler)
                        );
                }, new[] { GatewayIntents.GuildMembers });
            }

            var guildUserJoinedHandler = _services.GetService<IGuildUserJoinedHandler>();
            if (guildUserJoinedHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.UserJoined += async (guildUser) =>
                        await _taskExceptionLogger.LogOnError(async () =>
                            await guildUserJoinedHandler.GuildUserJoinedAsync(guildUser),
                            nameof(IGuildUserJoinedHandler)
                        );
                }, new[] { GatewayIntents.GuildMembers });
            }

            var guildUserLeftHandler = _services.GetService<IGuildUserLeftHandler>();
            if (guildUserLeftHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.UserLeft += async (guildUser) =>
                        await _taskExceptionLogger.LogOnError(async () =>
                            await guildUserLeftHandler.GuildUserLeftAsync(guildUser),
                            nameof(IGuildUserLeftHandler)
                        );
                }, new[] { GatewayIntents.GuildMembers });
            }
        }

        private IEnumerable<EventHandlerRegistrar> GetGuildBansRegistrars()
        {
            var guildUserBannedHandler = _services.GetService<IGuildUserBannedHandler>();
            if (guildUserBannedHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.UserBanned += async (user, guild) =>
                        await _taskExceptionLogger.LogOnError(async () =>
                            await guildUserBannedHandler.GuildUserBannedAsync(user, guild),
                            nameof(IGuildUserBannedHandler)
                        );
                }, new[] { GatewayIntents.GuildBans });
            }

            var guildUserUnbannedHandler = _services.GetService<IGuildUserUnbannedHandler>();
            if (guildUserUnbannedHandler != null)
            {
                yield return new EventHandlerRegistrar((client) =>
                {
                    client.DiscordShardedClient.UserUnbanned += async (user, guild) =>
                        await _taskExceptionLogger.LogOnError(async () =>
                            await guildUserUnbannedHandler.GuildUserUnbannedAsync(user, guild),
                            nameof(IGuildUserUnbannedHandler)
                        );
                }, new[] { GatewayIntents.GuildBans });
            }
        }
    }
}
