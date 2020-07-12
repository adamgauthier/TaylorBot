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
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<TaylorBotHostedService> logger;
        private readonly TaskExceptionLogger taskExceptionLogger;
        private ITaylorBotClient? client;

        public TaylorBotHostedService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.logger = serviceProvider.GetRequiredService<ILogger<TaylorBotHostedService>>();
            this.taskExceptionLogger = serviceProvider.GetRequiredService<TaskExceptionLogger>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            client = serviceProvider.GetRequiredService<ITaylorBotClient>();

            var shardReadyHandler = serviceProvider.GetService<IShardReadyHandler>();
            if (shardReadyHandler != null)
            {
                client.DiscordShardedClient.ShardReady += async (socketClient) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await shardReadyHandler.ShardReadyAsync(socketClient), nameof(IShardReadyHandler)
                    );
            }

            var allReadyHandler = serviceProvider.GetService<IAllReadyHandler>();
            if (allReadyHandler != null)
            {
                client.AllShardsReady += async () =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await allReadyHandler.AllShardsReadyAsync(), nameof(IAllReadyHandler)
                    );
            }

            var userMessageReceivedHandler = serviceProvider.GetService<IUserMessageReceivedHandler>();
            if (userMessageReceivedHandler != null)
            {
                client.DiscordShardedClient.MessageReceived += async (message) =>
                {
                    if (message is SocketUserMessage userMessage)
                    {
                        await taskExceptionLogger.LogOnError(async () =>
                            await userMessageReceivedHandler.UserMessageReceivedAsync(userMessage), nameof(IUserMessageReceivedHandler)
                        );
                    }
                };
            }

            var messageDeletedHandler = serviceProvider.GetService<IMessageDeletedHandler>();
            if (messageDeletedHandler != null)
            {
                client.DiscordShardedClient.MessageDeleted += async (message, channel) =>
                {
                    await taskExceptionLogger.LogOnError(async () =>
                        await messageDeletedHandler.UserMessageDeletedAsync(message, channel), nameof(IMessageDeletedHandler)
                    );
                };
            }

            var userUpdatedHandler = serviceProvider.GetService<IUserUpdatedHandler>();
            if (userUpdatedHandler != null)
            {
                client.DiscordShardedClient.UserUpdated += async (oldUser, newUser) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await userUpdatedHandler.UserUpdatedAsync(oldUser, newUser), nameof(IUserUpdatedHandler)
                    );
            }

            foreach (var joinedGuildHandler in serviceProvider.GetServices<IJoinedGuildHandler>())
            {
                client.DiscordShardedClient.JoinedGuild += async (guild) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await joinedGuildHandler.JoinedGuildAsync(guild), nameof(IJoinedGuildHandler)
                    );
            }

            var guildUpdatedHandler = serviceProvider.GetService<IGuildUpdatedHandler>();
            if (guildUpdatedHandler != null)
            {
                client.DiscordShardedClient.GuildUpdated += async (oldGuild, newGuild) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await guildUpdatedHandler.GuildUpdatedAsync(oldGuild, newGuild), nameof(IGuildUpdatedHandler)
                    );
            }

            var guildUserJoinedHandler = serviceProvider.GetService<IGuildUserJoinedHandler>();
            if (guildUserJoinedHandler != null)
            {
                client.DiscordShardedClient.UserJoined += async (guildUser) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await guildUserJoinedHandler.GuildUserJoinedAsync(guildUser), nameof(IGuildUserJoinedHandler)
                    );
            }

            var guildUserLeftHandler = serviceProvider.GetService<IGuildUserLeftHandler>();
            if (guildUserLeftHandler != null)
            {
                client.DiscordShardedClient.UserLeft += async (guildUser) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await guildUserLeftHandler.GuildUserLeftAsync(guildUser), nameof(IGuildUserLeftHandler)
                    );
            }

            var guildUserBannedHandler = serviceProvider.GetService<IGuildUserBannedHandler>();
            if (guildUserBannedHandler != null)
            {
                client.DiscordShardedClient.UserBanned += async (user, guild) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await guildUserBannedHandler.GuildUserBannedAsync(user, guild), nameof(IGuildUserBannedHandler)
                    );
            }

            var guildUserUnbannedHandler = serviceProvider.GetService<IGuildUserUnbannedHandler>();
            if (guildUserUnbannedHandler != null)
            {
                client.DiscordShardedClient.UserUnbanned += async (user, guild) =>
                    await taskExceptionLogger.LogOnError(async () =>
                        await guildUserUnbannedHandler.GuildUserUnbannedAsync(user, guild), nameof(IGuildUserUnbannedHandler)
                    );
            }

            var textChannelCreatedHandler = serviceProvider.GetService<ITextChannelCreatedHandler>();
            if (textChannelCreatedHandler != null)
            {
                client.DiscordShardedClient.ChannelCreated += async (socketChannel) =>
                {
                    if (socketChannel is SocketTextChannel textChannel)
                    {
                        await taskExceptionLogger.LogOnError(async () =>
                            await textChannelCreatedHandler.TextChannelCreatedAsync(textChannel), nameof(ITextChannelCreatedHandler)
                        );
                    }
                };
            }

            // Wait to login in case of a boot loop
            await Task.Delay(TimeSpan.FromSeconds(5));

            await client.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (client != null)
            {
                await client.StopAsync();
                logger.LogInformation(LogString.From("Clients unloaded!"));
            }
        }
    }
}
