using Discord.WebSocket;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.InstagramNotifier.Domain;
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
        private readonly SingletonTaskRunner instagramSingletonTaskRunner;
        private readonly RedditNotifierService redditNotiferService;
        private readonly YoutubeNotifierService youtubeNotiferService;
        private readonly TumblrNotifierService tumblrNotifierService;
        private readonly InstagramNotifierService instagramNotifierService;

        public ReadyHandler(
            SingletonTaskRunner redditSingletonTaskRunner,
            SingletonTaskRunner youtubeSingletonTaskRunner,
            SingletonTaskRunner tumblrSingletonTaskRunner,
            SingletonTaskRunner instagramSingletonTaskRunner,
            RedditNotifierService redditNotiferService,
            YoutubeNotifierService youtubeNotiferService,
            TumblrNotifierService tumblrNotifierService,
            InstagramNotifierService instagramNotifierService
        )
        {
            this.redditSingletonTaskRunner = redditSingletonTaskRunner;
            this.youtubeSingletonTaskRunner = youtubeSingletonTaskRunner;
            this.tumblrSingletonTaskRunner = tumblrSingletonTaskRunner;
            this.instagramSingletonTaskRunner = instagramSingletonTaskRunner;
            this.redditNotiferService = redditNotiferService;
            this.youtubeNotiferService = youtubeNotiferService;
            this.tumblrNotifierService = tumblrNotifierService;
            this.instagramNotifierService = instagramNotifierService;
        }

        public Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            redditSingletonTaskRunner.StartTaskIfNotStarted(async () => await redditNotiferService.StartRedditCheckerAsync());
            youtubeSingletonTaskRunner.StartTaskIfNotStarted(async () => await youtubeNotiferService.StartYoutubeCheckerAsync());
            tumblrSingletonTaskRunner.StartTaskIfNotStarted(async () => await tumblrNotifierService.StartTumblrCheckerAsync());
            tumblrSingletonTaskRunner.StartTaskIfNotStarted(async () => await tumblrNotifierService.StartTumblrCheckerAsync());
            instagramSingletonTaskRunner.StartTaskIfNotStarted(async () => await instagramNotifierService.StartInstagramCheckerAsync());

            return Task.CompletedTask;
        }
    }
}
