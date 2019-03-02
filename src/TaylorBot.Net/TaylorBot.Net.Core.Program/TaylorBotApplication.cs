using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Client;

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

            // Wait 5 seconds to login in case of a boot loop
            await Task.Delay(new TimeSpan(0, 0, 5));

            await client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
