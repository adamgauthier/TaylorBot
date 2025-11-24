using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.TumblrNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.TumblrNotifier.Domain.Options;

namespace TaylorBot.Net.TumblrNotifier.Domain;

public partial class TumblrNotifierService(
    ILogger<TumblrNotifierService> logger,
    IOptionsMonitor<TumblrNotifierOptions> optionsMonitor,
    ITumblrCheckerRepository tumblrCheckerRepository,
    TumblrClient tumblrClient,
    TumblrPostToEmbedMapper tumblrPostToEmbedMapper,
    Lazy<ITaylorBotClient> taylorBotClient
    )
{
    public async Task StartCheckingTumblrsAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(30));

        while (true)
        {
            try
            {
                await CheckAllTumblrsAsync();
            }
            catch (Exception e)
            {
                LogUnhandledExceptionCheckingTumblrs(e);
            }
            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
        }
    }

    public async ValueTask CheckAllTumblrsAsync()
    {
        foreach (var tumblrChecker in await tumblrCheckerRepository.GetTumblrCheckersAsync())
        {
            try
            {
                var channel = taylorBotClient.Value.ResolveRequiredGuild(tumblrChecker.GuildId).GetRequiredTextChannel(tumblrChecker.ChannelId);

                var response = await tumblrClient.GetPostsAsync(blogName: tumblrChecker.BlogName, filter: PostFilter.Text, count: 1);
                var blog = response.Blog;
                var newestPost = response.Result.Single();

                if (newestPost.ShortUrl != tumblrChecker.LastPostShortUrl)
                {
                    LogFoundNewTumblrPost(tumblrChecker, newestPost.ShortUrl);
                    await channel.SendMessageAsync(embed: tumblrPostToEmbedMapper.ToEmbed(newestPost, blog));
                    await tumblrCheckerRepository.UpdateLastPostAsync(tumblrChecker, newestPost);
                }
            }
            catch (Exception exception)
            {
                LogExceptionCheckingTumblr(exception, tumblrChecker);
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Found new Tumblr post for {TumblrChecker}: {PostShortUrl}.")]
    private partial void LogFoundNewTumblrPost(TumblrChecker tumblrChecker, string postShortUrl);

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred when checking {TumblrChecker}.")]
    private partial void LogExceptionCheckingTumblr(Exception exception, TumblrChecker tumblrChecker);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in " + nameof(CheckAllTumblrsAsync) + ".")]
    private partial void LogUnhandledExceptionCheckingTumblrs(Exception exception);
}
