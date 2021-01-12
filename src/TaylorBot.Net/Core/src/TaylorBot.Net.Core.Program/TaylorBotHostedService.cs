using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaylorBotHostedService> _logger;
        private readonly TaskExceptionLogger _taskExceptionLogger;
        private ITaylorBotClient? _client;

        public TaylorBotHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<TaylorBotHostedService>>();
            _taskExceptionLogger = serviceProvider.GetRequiredService<TaskExceptionLogger>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client = _serviceProvider.GetRequiredService<ITaylorBotClient>();

            var shardReadyHandler = _serviceProvider.GetService<IShardReadyHandler>();
            if (shardReadyHandler != null)
            {
                _client.DiscordShardedClient.ShardReady += async (socketClient) =>
                    await _taskExceptionLogger.LogOnError(async () =>
                        await shardReadyHandler.ShardReadyAsync(socketClient),
                        nameof(IShardReadyHandler)
                    );
            }

            var allReadyHandler = _serviceProvider.GetService<IAllReadyHandler>();
            if (allReadyHandler != null)
            {
                _client.AllShardsReady += async () =>
                    await _taskExceptionLogger.LogOnError(async () =>
                        await allReadyHandler.AllShardsReadyAsync(),
                        nameof(IAllReadyHandler)
                    );
            }

            var userMessageReceivedHandler = _serviceProvider.GetService<IUserMessageReceivedHandler>();
            if (userMessageReceivedHandler != null)
            {
                _client.DiscordShardedClient.MessageReceived += async (message) =>
                {
                    if (message is SocketUserMessage userMessage)
                    {
                        await _taskExceptionLogger.LogOnError(async () =>
                            await userMessageReceivedHandler.UserMessageReceivedAsync(userMessage),
                            nameof(IUserMessageReceivedHandler)
                        );
                    }
                };
            }

            var messageDeletedHandler = _serviceProvider.GetService<IMessageDeletedHandler>();
            if (messageDeletedHandler != null)
            {
                _client.DiscordShardedClient.MessageDeleted += async (message, channel) =>
                {
                    await _taskExceptionLogger.LogOnError(async () =>
                        await messageDeletedHandler.MessageDeletedAsync(message, channel),
                        nameof(IMessageDeletedHandler)
                    );
                };
            }

            var messageBulkDeletedHandler = _serviceProvider.GetService<IMessageBulkDeletedHandler>();
            if (messageBulkDeletedHandler != null)
            {
                _client.DiscordShardedClient.MessagesBulkDeleted += async (messages, channel) =>
                {
                    await _taskExceptionLogger.LogOnError(async () =>
                        await messageBulkDeletedHandler.MessageBulkDeletedAsync(messages, channel),
                        nameof(IMessageBulkDeletedHandler)
                    );
                };
            }

            var userUpdatedHandler = _serviceProvider.GetService<IUserUpdatedHandler>();
            if (userUpdatedHandler != null)
            {
                _client.DiscordShardedClient.UserUpdated += async (oldUser, newUser) =>
                    await _taskExceptionLogger.LogOnError(async () =>
                        await userUpdatedHandler.UserUpdatedAsync(oldUser, newUser),
                        nameof(IUserUpdatedHandler)
                    );
            }

            foreach (var joinedGuildHandler in _serviceProvider.GetServices<IJoinedGuildHandler>())
            {
                _client.DiscordShardedClient.JoinedGuild += async (guild) =>
                    await _taskExceptionLogger.LogOnError(async () =>
                        await joinedGuildHandler.JoinedGuildAsync(guild),
                        nameof(IJoinedGuildHandler)
                    );
            }

            var guildUpdatedHandler = _serviceProvider.GetService<IGuildUpdatedHandler>();
            if (guildUpdatedHandler != null)
            {
                _client.DiscordShardedClient.GuildUpdated += async (oldGuild, newGuild) =>
                    await _taskExceptionLogger.LogOnError(async () =>
                        await guildUpdatedHandler.GuildUpdatedAsync(oldGuild, newGuild),
                        nameof(IGuildUpdatedHandler)
                    );
            }

            var guildUserJoinedHandler = _serviceProvider.GetService<IGuildUserJoinedHandler>();
            if (guildUserJoinedHandler != null)
            {
                _client.DiscordShardedClient.UserJoined += async (guildUser) =>
                    await _taskExceptionLogger.LogOnError(async () =>
                        await guildUserJoinedHandler.GuildUserJoinedAsync(guildUser),
                        nameof(IGuildUserJoinedHandler)
                    );
            }

            var guildUserLeftHandler = _serviceProvider.GetService<IGuildUserLeftHandler>();
            if (guildUserLeftHandler != null)
            {
                _client.DiscordShardedClient.UserLeft += async (guildUser) =>
                    await _taskExceptionLogger.LogOnError(async () =>
                        await guildUserLeftHandler.GuildUserLeftAsync(guildUser),
                        nameof(IGuildUserLeftHandler)
                    );
            }

            var guildUserBannedHandler = _serviceProvider.GetService<IGuildUserBannedHandler>();
            if (guildUserBannedHandler != null)
            {
                _client.DiscordShardedClient.UserBanned += async (user, guild) =>
                    await _taskExceptionLogger.LogOnError(async () =>
                        await guildUserBannedHandler.GuildUserBannedAsync(user, guild),
                        nameof(IGuildUserBannedHandler)
                    );
            }

            var guildUserUnbannedHandler = _serviceProvider.GetService<IGuildUserUnbannedHandler>();
            if (guildUserUnbannedHandler != null)
            {
                _client.DiscordShardedClient.UserUnbanned += async (user, guild) =>
                    await _taskExceptionLogger.LogOnError(async () =>
                        await guildUserUnbannedHandler.GuildUserUnbannedAsync(user, guild),
                        nameof(IGuildUserUnbannedHandler)
                    );
            }

            var textChannelCreatedHandler = _serviceProvider.GetService<ITextChannelCreatedHandler>();
            if (textChannelCreatedHandler != null)
            {
                _client.DiscordShardedClient.ChannelCreated += async (socketChannel) =>
                {
                    if (socketChannel is SocketTextChannel textChannel)
                    {
                        await _taskExceptionLogger.LogOnError(async () =>
                            await textChannelCreatedHandler.TextChannelCreatedAsync(textChannel),
                            nameof(ITextChannelCreatedHandler)
                        );
                    }
                };
            }

            var reactionAddedHandler = _serviceProvider.GetService<IReactionAddedHandler>();
            if (reactionAddedHandler != null)
            {
                _client.DiscordShardedClient.ReactionAdded += async (message, channel, reaction) =>
                    await _taskExceptionLogger.LogOnError(async () =>
                        await reactionAddedHandler.ReactionAddedAsync(message, channel, reaction),
                        nameof(IReactionAddedHandler)
                    );
            }

            var reactionRemovedHandler = _serviceProvider.GetService<IReactionRemovedHandler>();
            if (reactionRemovedHandler != null)
            {
                _client.DiscordShardedClient.ReactionRemoved += async (message, channel, reaction) =>
                    await _taskExceptionLogger.LogOnError(async () =>
                        await reactionRemovedHandler.ReactionRemovedAsync(message, channel, reaction),
                        nameof(IReactionRemovedHandler)
                    );
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
    }
}
