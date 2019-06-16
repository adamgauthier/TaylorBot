using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Client;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Logging;
using System.Runtime.Loader;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Core.Program
{
    public class TaylorBotApplication
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<TaylorBotApplication> logger;
        private readonly TaskExceptionLogger taskExceptionLogger;

        public TaylorBotApplication(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.logger = serviceProvider.GetRequiredService<ILogger<TaylorBotApplication>>();
            this.taskExceptionLogger = serviceProvider.GetRequiredService<TaskExceptionLogger>();
        }

        public async Task StartAsync()
        {
            var client = serviceProvider.GetRequiredService<TaylorBotClient>();

            AssemblyLoadContext.Default.Unloading += async (assemblyLoadContext) =>
            {
                await client.StopAsync();
                logger.LogInformation(LogString.From("Clients unloaded!"));
            };

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

            // Wait 5 seconds to login in case of a boot loop
            await Task.Delay(new TimeSpan(0, 0, 5));

            await client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
