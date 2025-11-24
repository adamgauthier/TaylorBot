using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.RedditNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.RedditNotifier.Domain.Options;

namespace TaylorBot.Net.RedditNotifier.Domain;

public partial class RedditNotifierService(
    IServiceProvider serviceProvider,
    ILogger<RedditNotifierService> logger,
    IOptionsMonitor<RedditNotifierOptions> optionsMonitor,
    IRedditCheckerRepository redditCheckerRepository,
    RedditPostToEmbedMapper redditPostToEmbedMapper,
    Lazy<ITaylorBotClient> taylorBotClient
    )
{
    public async Task StartCheckingRedditsAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(30));

        while (true)
        {
            try
            {
                await CheckAllRedditsAsync();
            }
            catch (Exception e)
            {
                LogUnhandledExceptionCheckingReddits(e);
            }
            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
        }
    }

    public async ValueTask CheckAllRedditsAsync()
    {
        var redditCheckers = await redditCheckerRepository.GetRedditCheckersAsync();
        LogFoundRedditCheckers(redditCheckers.Count);

        Lazy<RedditHttpClient> redditClient = new(valueFactory: serviceProvider.GetRequiredService<RedditHttpClient>);

        foreach (var redditChecker in redditCheckers)
        {
            try
            {
                var channel = taylorBotClient.Value.ResolveRequiredGuild(redditChecker.GuildId).GetRequiredTextChannel(redditChecker.ChannelId);

                var newestPost = await redditClient.Value.GetNewestPostAsync(redditChecker.SubredditName);

                if (redditChecker.LastPostId == null || !redditChecker.LastPostCreatedAt.HasValue ||
                    (newestPost.id != redditChecker.LastPostId && newestPost.CreatedAt > redditChecker.LastPostCreatedAt.Value))
                {
                    LogFoundNewRedditPost(redditChecker, newestPost.id);
                    await channel.SendMessageAsync(embed: redditPostToEmbedMapper.ToEmbed(newestPost));
                    await redditCheckerRepository.UpdateLastPostAsync(redditChecker, newestPost);
                }
            }
            catch (Exception exception)
            {
                LogExceptionCheckingReddit(exception, redditChecker);
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Found {Count} reddit checkers")]
    private partial void LogFoundRedditCheckers(int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Found new Reddit post for {RedditChecker}: {PostId}.")]
    private partial void LogFoundNewRedditPost(RedditChecker redditChecker, string postId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred when checking {RedditChecker}.")]
    private partial void LogExceptionCheckingReddit(Exception exception, RedditChecker redditChecker);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in " + nameof(CheckAllRedditsAsync) + ".")]
    private partial void LogUnhandledExceptionCheckingReddits(Exception exception);
}
