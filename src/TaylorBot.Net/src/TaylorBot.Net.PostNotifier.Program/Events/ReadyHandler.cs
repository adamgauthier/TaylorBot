using Discord.WebSocket;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.YoutubeNotifier.Domain;

namespace TaylorBot.Net.PostNotifier.Program.Events
{
    public class ReadyHandler : IShardReadyHandler
    {
        private readonly SingletonTaskRunner redditSingletonTaskRunner;
        private readonly SingletonTaskRunner youtubeSingletonTaskRunner;
        private readonly RedditNotifierService redditNotiferService;
        private readonly YoutubeNotifierService youtubeNotiferService;

        public ReadyHandler(
            SingletonTaskRunner redditSingletonTaskRunner,
            SingletonTaskRunner youtubeSingletonTaskRunner,
            RedditNotifierService redditNotiferService,
            YoutubeNotifierService youtubeNotiferService)
        {
            this.redditSingletonTaskRunner = redditSingletonTaskRunner;
            this.youtubeSingletonTaskRunner = youtubeSingletonTaskRunner;
            this.redditNotiferService = redditNotiferService;
            this.youtubeNotiferService = youtubeNotiferService;
        }

        public Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            redditSingletonTaskRunner.StartTaskIfNotStarted(async () => await redditNotiferService.StartRedditCheckerAsync());
            youtubeSingletonTaskRunner.StartTaskIfNotStarted(async () => await youtubeNotiferService.StartRedditCheckerAsync());

            return Task.CompletedTask;
        }
    }
}
