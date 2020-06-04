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
        private readonly ILogger<RedditNotifierService> _logger;
        private readonly IOptionsMonitor<RedditNotifierOptions> _optionsMonitor;
        private readonly IRedditCheckerRepository _redditCheckerRepository;
        private readonly RedditClient _redditClient;
        private readonly RedditPostToEmbedMapper _redditPostToEmbedMapper;
        private readonly ITaylorBotClient _taylorBotClient;

        public RedditNotifierService(
            ILogger<RedditNotifierService> logger,
            IOptionsMonitor<RedditNotifierOptions> optionsMonitor,
            IRedditCheckerRepository redditCheckerRepository,
            RedditClient redditClient,
            RedditPostToEmbedMapper redditPostToEmbedMapper,
            ITaylorBotClient taylorBotClient)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _redditCheckerRepository = redditCheckerRepository;
            _redditClient = redditClient;
            _redditPostToEmbedMapper = redditPostToEmbedMapper;
            _taylorBotClient = taylorBotClient;
        }

        public async Task StartCheckingRedditsAsync()
        {
            while (true)
            {
                try
                {
                    await CheckAllRedditsAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, LogString.From($"Unhandled exception in {nameof(CheckAllRedditsAsync)}."));
                }
                await Task.Delay(_optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
            }
        }

        public async ValueTask CheckAllRedditsAsync()
        {
            foreach (var redditChecker in await _redditCheckerRepository.GetRedditCheckersAsync())
            {
                try
                {
                    var channel = _taylorBotClient.ResolveRequiredGuild(redditChecker.GuildId).GetRequiredTextChannel(redditChecker.ChannelId);

                    var newestPost = _redditClient.Subreddit(name: redditChecker.SubredditName).Posts.GetNew(limit: 1).Single();

                    if (redditChecker.LastPostId == null || !redditChecker.LastPostCreatedAt.HasValue ||
                        (newestPost.Id != redditChecker.LastPostId && newestPost.Created > redditChecker.LastPostCreatedAt.Value))
                    {
                        _logger.LogTrace(LogString.From($"Found new Reddit post for {redditChecker}: {newestPost.Id}."));
                        await channel.SendMessageAsync(embed: _redditPostToEmbedMapper.ToEmbed(newestPost));
                        await _redditCheckerRepository.UpdateLastPostAsync(redditChecker, newestPost);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, LogString.From($"Exception occurred when checking {redditChecker}."));
                }

                await Task.Delay(_optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
            }
        }
    }
}
