using Discord.WebSocket;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.TumblrNotifier.Domain;
using TaylorBot.Net.YoutubeNotifier.Domain;

namespace TaylorBot.Net.PostNotifier.Program.Events
{
    public class ReadyHandler : IShardReadyHandler
    {
        private readonly SingletonTaskRunner redditSingletonTaskRunner;
        private readonly SingletonTaskRunner youtubeSingletonTaskRunner;
        private readonly SingletonTaskRunner tumblrSingletonTaskRunner;
        private readonly RedditNotifierService redditNotiferService;
        private readonly YoutubeNotifierService youtubeNotiferService;
        private readonly TumblrNotifierService tumblrNotifierService;

        public ReadyHandler(
            SingletonTaskRunner redditSingletonTaskRunner,
            SingletonTaskRunner youtubeSingletonTaskRunner,
            SingletonTaskRunner tumblrSingletonTaskRunner,
            RedditNotifierService redditNotiferService,
            YoutubeNotifierService youtubeNotiferService,
            TumblrNotifierService tumblrNotifierService)
        {
            this.redditSingletonTaskRunner = redditSingletonTaskRunner;
            this.youtubeSingletonTaskRunner = youtubeSingletonTaskRunner;
            this.tumblrSingletonTaskRunner = tumblrSingletonTaskRunner;
            this.redditNotiferService = redditNotiferService;
            this.youtubeNotiferService = youtubeNotiferService;
            this.tumblrNotifierService = tumblrNotifierService;
        }

        public Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            redditSingletonTaskRunner.StartTaskIfNotStarted(async () => await redditNotiferService.StartRedditCheckerAsync());
            youtubeSingletonTaskRunner.StartTaskIfNotStarted(async () => await youtubeNotiferService.StartYoutubeCheckerAsync());
            tumblrSingletonTaskRunner.StartTaskIfNotStarted(async () => await tumblrNotifierService.StartTumblrCheckerAsync());

            return Task.CompletedTask;
        }
    }
}
