using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.RedditNotifier.Domain;

namespace TaylorBot.Net.PostNotifier.Program.Events
{
    public class ReadyHandler : IReadyHandler
    {
        private readonly ILogger<ReadyHandler> logger;
        private readonly TaylorBotClient taylorBotClient;
        private readonly SingletonTaskRunner singletonTaskRunner;
        private readonly RedditNotiferDomainService redditNotiferApplicationService;

        public ReadyHandler(ILogger<ReadyHandler> logger, TaylorBotClient taylorBotClient, SingletonTaskRunner singletonTaskRunner, RedditNotiferDomainService redditNotiferApplicationService)
        {
            this.logger = logger;
            this.taylorBotClient = taylorBotClient;
            this.singletonTaskRunner = singletonTaskRunner;
            this.redditNotiferApplicationService = redditNotiferApplicationService;
        }

        public Task ReadyAsync(DiscordSocketClient shardClient)
        {
            logger.LogInformation(LogString.From(
                $"Shard Number {shardClient.ShardId} is ready! Serving {"guild".DisplayCount(shardClient.Guilds.Count)} out of {taylorBotClient.DiscordShardedClient.Guilds.Count}."
            ));

            singletonTaskRunner.StartTaskIfNotStarted(async () => await redditNotiferApplicationService.StartRedditCheckerAsync());

            return Task.CompletedTask;
        }
    }
}
