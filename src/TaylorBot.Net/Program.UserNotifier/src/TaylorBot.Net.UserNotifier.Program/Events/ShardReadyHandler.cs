using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.InstagramNotifier.Domain;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.TumblrNotifier.Domain;
using TaylorBot.Net.YoutubeNotifier.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class ShardReadyHandler(
    SingletonTaskRunner redditSingletonTaskRunner,
    SingletonTaskRunner youtubeSingletonTaskRunner,
    SingletonTaskRunner tumblrSingletonTaskRunner,
    SingletonTaskRunner instagramSingletonTaskRunner,
    RedditNotifierService redditNotiferService,
    YoutubeNotifierService youtubeNotiferService,
    TumblrNotifierService tumblrNotifierService,
    InstagramNotifierService instagramNotifierService
    ) : IShardReadyHandler
{
    public Task ShardReadyAsync(DiscordSocketClient shardClient)
    {
        _ = redditSingletonTaskRunner.StartTaskIfNotStarted(
            redditNotiferService.StartCheckingRedditsAsync,
            nameof(RedditNotifierService)
        );

        _ = youtubeSingletonTaskRunner.StartTaskIfNotStarted(
            youtubeNotiferService.StartCheckingYoutubesAsync,
            nameof(YoutubeNotifierService)
        );

        _ = tumblrSingletonTaskRunner.StartTaskIfNotStarted(
            tumblrNotifierService.StartCheckingTumblrsAsync,
            nameof(TumblrNotifierService)
        );

        _ = instagramSingletonTaskRunner.StartTaskIfNotStarted(
            instagramNotifierService.StartCheckingInstagramsAsync,
            nameof(InstagramNotifierService)
        );

        return Task.CompletedTask;
    }
}
