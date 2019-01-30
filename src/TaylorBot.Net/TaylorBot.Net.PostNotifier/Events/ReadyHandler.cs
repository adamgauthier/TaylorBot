using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaylorBot.Net.Application.Events;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.PostNotifier.Events
{
    public class ReadyHandler : IReadyHandler
    {
        private readonly TaylorBotClient taylorBotClient;
        private readonly ILogger<ReadyHandler> logger;

        public ReadyHandler(TaylorBotClient taylorBotClient, ILogger<ReadyHandler> logger)
        {
            this.taylorBotClient = taylorBotClient;
            this.logger = logger;
        }

        public Task ReadyAsync(DiscordSocketClient shardClient)
        {
            logger.LogInformation(LogString.From(
                $"Shard Number {shardClient.ShardId} is ready! Serving {shardClient.Guilds.Count} guilds out of {taylorBotClient.DiscordShardedClient.Guilds.Count}."
            ));

            return Task.CompletedTask;
        }
    }
}
