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

            var readyHandler = serviceProvider.GetService<IReadyHandler>();
            if (readyHandler != null)
                client.DiscordShardedClient.ShardReady += readyHandler.ReadyAsync;

            // Wait 5 seconds to login in case of a boot loop
            await Task.Delay(new TimeSpan(0, 0, 5));

            await client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
