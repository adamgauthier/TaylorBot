using Discord.WebSocket;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.InstagramNotifier.Domain;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.TumblrNotifier.Domain;
using TaylorBot.Net.YoutubeNotifier.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events
{
    public class ShardReadyHandler : IShardReadyHandler
    {
        private readonly SingletonTaskRunner _redditSingletonTaskRunner;
        private readonly SingletonTaskRunner _youtubeSingletonTaskRunner;
        private readonly SingletonTaskRunner _tumblrSingletonTaskRunner;
        private readonly SingletonTaskRunner _instagramSingletonTaskRunner;
        private readonly RedditNotifierService _redditNotiferService;
        private readonly YoutubeNotifierService _youtubeNotiferService;
        private readonly TumblrNotifierService _tumblrNotifierService;
        private readonly InstagramNotifierService _instagramNotifierService;
        private readonly TaskExceptionLogger _taskExceptionLogger;

        public ShardReadyHandler(
            SingletonTaskRunner redditSingletonTaskRunner,
            SingletonTaskRunner youtubeSingletonTaskRunner,
            SingletonTaskRunner tumblrSingletonTaskRunner,
            SingletonTaskRunner instagramSingletonTaskRunner,
            RedditNotifierService redditNotiferService,
            YoutubeNotifierService youtubeNotiferService,
            TumblrNotifierService tumblrNotifierService,
            InstagramNotifierService instagramNotifierService,
            TaskExceptionLogger taskExceptionLogger
        )
        {
            _redditSingletonTaskRunner = redditSingletonTaskRunner;
            _youtubeSingletonTaskRunner = youtubeSingletonTaskRunner;
            _tumblrSingletonTaskRunner = tumblrSingletonTaskRunner;
            _instagramSingletonTaskRunner = instagramSingletonTaskRunner;
            _redditNotiferService = redditNotiferService;
            _youtubeNotiferService = youtubeNotiferService;
            _tumblrNotifierService = tumblrNotifierService;
            _instagramNotifierService = instagramNotifierService;
            _taskExceptionLogger = taskExceptionLogger;
        }

        public Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            _ = _redditSingletonTaskRunner.StartTaskIfNotStarted(
                _redditNotiferService.StartCheckingRedditsAsync,
                nameof(RedditNotifierService)
            );

            _ = _youtubeSingletonTaskRunner.StartTaskIfNotStarted(
                _youtubeNotiferService.StartCheckingYoutubesAsync,
                nameof(YoutubeNotifierService)
            );

            _ = _tumblrSingletonTaskRunner.StartTaskIfNotStarted(
                _tumblrNotifierService.StartCheckingTumblrsAsync,
                nameof(TumblrNotifierService)
            );

            _ = _instagramSingletonTaskRunner.StartTaskIfNotStarted(
                _instagramNotifierService.StartCheckingInstagramsAsync,
                nameof(InstagramNotifierService)
            );

            return Task.CompletedTask;
        }
    }
}
