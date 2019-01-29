using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Application.Events;
using TaylorBot.Net.Core;

namespace TaylorBot.Net.PostNotifier.Events
{
    public class ReadyHandler : IReadyHandler
    {
        private readonly TaylorBotClient taylorBotClient;

        public ReadyHandler(TaylorBotClient taylorBotClient)
        {
            this.taylorBotClient = taylorBotClient;
        }

        public Task ReadyAsync(DiscordSocketClient shardClient)
        {
            Console.WriteLine($"Shard Number {shardClient.ShardId} is connected and ready! Serving {shardClient.Guilds.Count} guilds out of {taylorBotClient.DiscordShardedClient.Guilds.Count}.");

            return Task.CompletedTask;
        }
    }
}
