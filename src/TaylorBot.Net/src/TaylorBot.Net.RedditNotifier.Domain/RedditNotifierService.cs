using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reddit;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.RedditNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.RedditNotifier.Domain.Options;

namespace TaylorBot.Net.RedditNotifier.Domain
{
    public class RedditNotifierService
    {
        private readonly ILogger<RedditNotifierService> logger;
        private readonly IOptionsMonitor<RedditNotifierOptions> optionsMonitor;
        private readonly IRedditCheckerRepository redditCheckerRepository;
        private readonly RedditAPI redditAPI;
        private readonly RedditPostToEmbedMapper redditPostToEmbedMapper;
        private readonly TaylorBotClient taylorBotClient;

        public RedditNotifierService(
            ILogger<RedditNotifierService> logger,
            IOptionsMonitor<RedditNotifierOptions> optionsMonitor,
            IRedditCheckerRepository redditCheckerRepository,
            RedditAPI redditAPI,
            RedditPostToEmbedMapper redditPostToEmbedMapper,
            TaylorBotClient taylorBotClient)
        {
            this.logger = logger;
            this.optionsMonitor = optionsMonitor;
            this.redditCheckerRepository = redditCheckerRepository;
            this.redditAPI = redditAPI;
            this.redditPostToEmbedMapper = redditPostToEmbedMapper;
            this.taylorBotClient = taylorBotClient;
        }

        public async Task StartRedditCheckerAsync()
        {
            while (true)
            {
                foreach (var redditChecker in await redditCheckerRepository.GetRedditCheckersAsync())
                {
                    try
                    {
                        var channel = taylorBotClient.ResolveRequiredGuild(redditChecker.GuildId).GetRequiredTextChannel(redditChecker.ChannelId);

                        var newestPost = redditAPI.Subreddit(name: redditChecker.SubredditName).Posts.GetNew(limit: 1).Single();

                        if (newestPost.Id != redditChecker.LastPostId && newestPost.Created > redditChecker.LastPostCreatedAt)
                        {
                            logger.LogTrace(LogString.From($"Found new Reddit post for {redditChecker}: {newestPost.Id}."));
                            await channel.SendMessageAsync(embed: redditPostToEmbedMapper.ToEmbed(newestPost));
                            await redditCheckerRepository.UpdateLastPostAsync(redditChecker, newestPost);
                        }
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, LogString.From($"Exception occurred when checking {redditChecker}."));
                    }

                    await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
                }

                await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
            }
        }
    }
}
