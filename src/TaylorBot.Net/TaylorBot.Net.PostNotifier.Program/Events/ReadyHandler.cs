using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.RedditNotifier.Domain;

namespace TaylorBot.Net.PostNotifier.Program.Events
{
    public class ReadyHandler : IShardReadyHandler
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

        public Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            singletonTaskRunner.StartTaskIfNotStarted(async () => await redditNotiferApplicationService.StartRedditCheckerAsync());

            return Task.CompletedTask;
        }
    }
}
