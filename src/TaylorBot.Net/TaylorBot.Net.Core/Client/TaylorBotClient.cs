using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Configuration;

namespace TaylorBot.Net.Core
{
    public class TaylorBotClient
    {
        private readonly ITokenProvider tokenProvider;

        public DiscordShardedClient DiscordShardedClient { get; }

        public TaylorBotClient(ITokenProvider tokenProvider, DiscordShardedClient discordShardedClient)
        {
            this.tokenProvider = tokenProvider;
            DiscordShardedClient = discordShardedClient;

            DiscordShardedClient.Log += LogAsync;
        }

        public async Task StartAsync()
        {
            await DiscordShardedClient.LoginAsync(TokenType.Bot, tokenProvider.GetDiscordToken());
            await DiscordShardedClient.StartAsync();
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}
