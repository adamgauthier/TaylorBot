using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.TumblrNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.TumblrNotifier.Domain.Options;

namespace TaylorBot.Net.TumblrNotifier.Domain;

public class TumblrNotifierService(
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
        while (true)
        {
            try
            {
                await CheckAllTumblrsAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unhandled exception in {nameof(CheckAllTumblrsAsync)}.");
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
                    logger.LogDebug($"Found new Tumblr post for {tumblrChecker}: {newestPost.ShortUrl}.");
                    await channel.SendMessageAsync(embed: tumblrPostToEmbedMapper.ToEmbed(newestPost, blog));
                    await tumblrCheckerRepository.UpdateLastPostAsync(tumblrChecker, newestPost);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Exception occurred when checking {tumblrChecker}.");
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
        }
    }
}
