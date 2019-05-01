using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Client;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Logging;
using System.Runtime.Loader;

namespace TaylorBot.Net.Core.Program
{
    public class TaylorBotApplication
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<TaylorBotApplication> logger;

        public TaylorBotApplication(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.logger = serviceProvider.GetRequiredService<ILogger<TaylorBotApplication>>();
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
                    await WrapTryCatch(shardReadyHandler.ShardReadyAsync(socketClient), nameof(IShardReadyHandler));
            }

            var allReadyHandler = serviceProvider.GetService<IAllReadyHandler>();
            if (allReadyHandler != null)
            {
                client.AllShardsReady += async () =>
                    await WrapTryCatch(allReadyHandler.AllShardsReadyAsync(), nameof(IAllReadyHandler));
            }

            var userMessageReceivedHandler = serviceProvider.GetService<IUserMessageReceivedHandler>();
            if (userMessageReceivedHandler != null)
            {
                client.DiscordShardedClient.MessageReceived += async (message) =>
                {
                    if (message is SocketUserMessage userMessage)
                    {
                        await WrapTryCatch(userMessageReceivedHandler.UserMessageReceivedAsync(userMessage), nameof(IUserMessageReceivedHandler));
                    }
                };
            }

            var userUpdatedHandler = serviceProvider.GetService<IUserUpdatedHandler>();
            if (userUpdatedHandler != null)
            {
                client.DiscordShardedClient.UserUpdated += async (oldUser, newUser) =>
                    await WrapTryCatch(userUpdatedHandler.UserUpdatedAsync(oldUser, newUser), nameof(IUserUpdatedHandler));
            }

            var joinedGuildHandler = serviceProvider.GetService<IJoinedGuildHandler>();
            if (joinedGuildHandler != null)
            {
                client.DiscordShardedClient.JoinedGuild += async (guild) =>
                    await WrapTryCatch(joinedGuildHandler.JoinedGuildAsync(guild), nameof(IJoinedGuildHandler));
            }

            // Wait 5 seconds to login in case of a boot loop
            await Task.Delay(new TimeSpan(0, 0, 5));

            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task WrapTryCatch(Task task, string handlerName)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, LogString.From($"Unhandled exception in {handlerName}."));
                throw;
            }
        }
    }
}
