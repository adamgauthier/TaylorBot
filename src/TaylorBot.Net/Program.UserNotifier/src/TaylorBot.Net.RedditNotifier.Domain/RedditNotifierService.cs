﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.RedditNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.RedditNotifier.Domain.Options;

namespace TaylorBot.Net.RedditNotifier.Domain;

public class RedditNotifierService(
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
                logger.LogError(e, $"Unhandled exception in {nameof(CheckAllRedditsAsync)}.");
            }
            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
        }
    }

    public async ValueTask CheckAllRedditsAsync()
    {
        var redditCheckers = await redditCheckerRepository.GetRedditCheckersAsync();
        logger.LogInformation("Found {Count} reddit checkers", redditCheckers.Count);

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
                    logger.LogDebug("Found new Reddit post for {RedditChecker}: {PostId}.", redditChecker, newestPost.id);
                    await channel.SendMessageAsync(embed: redditPostToEmbedMapper.ToEmbed(newestPost));
                    await redditCheckerRepository.UpdateLastPostAsync(redditChecker, newestPost);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Exception occurred when checking {RedditChecker}.", redditChecker);
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
        }
    }
}
