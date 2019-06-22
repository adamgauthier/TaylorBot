using Discord.WebSocket;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.RedditNotifier.Domain;

namespace TaylorBot.Net.PostNotifier.Program.Events
{
    public class ReadyHandler : IShardReadyHandler
    {
        private readonly SingletonTaskRunner singletonTaskRunner;
        private readonly RedditNotifierService redditNotiferApplicationService;

        public ReadyHandler(SingletonTaskRunner singletonTaskRunner, RedditNotifierService redditNotiferApplicationService)
        {
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
