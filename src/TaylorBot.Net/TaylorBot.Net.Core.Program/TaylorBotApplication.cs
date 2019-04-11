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

        public TaylorBotApplication(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task StartAsync()
        {
            var client = serviceProvider.GetRequiredService<TaylorBotClient>();
            var logger = serviceProvider.GetRequiredService<ILogger<TaylorBotApplication>>();

            AssemblyLoadContext.Default.Unloading += async (assemblyLoadContext) =>
            {
                await client.StopAsync();
                logger.LogInformation(LogString.From("Clients unloaded!"));
            };

            var shardReadyHandler = serviceProvider.GetService<IShardReadyHandler>();
            if (shardReadyHandler != null)
            {
                client.DiscordShardedClient.ShardReady += shardReadyHandler.ShardReadyAsync;
            }

            var allReadyHandler = serviceProvider.GetService<IAllReadyHandler>();
            if (allReadyHandler != null)
            {
                client.AllShardsReady += allReadyHandler.AllShardsReadyAsync;
            }

            var userMessageReceivedHandler = serviceProvider.GetService<IUserMessageReceivedHandler>();
            if (userMessageReceivedHandler != null)
            {
                client.DiscordShardedClient.MessageReceived += async (message) =>
                {
                    if (message is SocketUserMessage userMessage)
                    {
                        try
                        {
                            await userMessageReceivedHandler.UserMessageReceivedAsync(userMessage);
                        }
                        catch (Exception exception)
                        {
                            logger.LogError(exception, LogString.From($"Unhandled exception in {nameof(IUserMessageReceivedHandler)}."));
                            throw;
                        }
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
