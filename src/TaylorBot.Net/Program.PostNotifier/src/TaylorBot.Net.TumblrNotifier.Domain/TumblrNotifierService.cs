using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.TumblrNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.TumblrNotifier.Domain.Options;

namespace TaylorBot.Net.TumblrNotifier.Domain
{
    public class TumblrNotifierService
    {
        private readonly ILogger<TumblrNotifierService> logger;
        private readonly IOptionsMonitor<TumblrNotifierOptions> optionsMonitor;
        private readonly ITumblrCheckerRepository tumblrCheckerRepository;
        private readonly TumblrClient tumblrClient;
        private readonly TumblrPostToEmbedMapper tumblrPostToEmbedMapper;
        private readonly TaylorBotClient taylorBotClient;

        public TumblrNotifierService(
            ILogger<TumblrNotifierService> logger,
            IOptionsMonitor<TumblrNotifierOptions> optionsMonitor,
            ITumblrCheckerRepository tumblrCheckerRepository,
            TumblrClient tumblrClient,
            TumblrPostToEmbedMapper tumblrPostToEmbedMapper,
            TaylorBotClient taylorBotClient)
        {
            this.logger = logger;
            this.optionsMonitor = optionsMonitor;
            this.tumblrCheckerRepository = tumblrCheckerRepository;
            this.tumblrClient = tumblrClient;
            this.tumblrPostToEmbedMapper = tumblrPostToEmbedMapper;
            this.taylorBotClient = taylorBotClient;
        }

        public async Task StartTumblrCheckerAsync()
        {
            while (true)
            {
                foreach (var tumblrChecker in await tumblrCheckerRepository.GetTumblrCheckersAsync())
                {
                    try
                    {
                        var channel = taylorBotClient.ResolveRequiredGuild(tumblrChecker.GuildId).GetRequiredTextChannel(tumblrChecker.ChannelId);

                        var response = await tumblrClient.GetPostsAsync(blogName: tumblrChecker.BlogName, filter: PostFilter.Text, count: 1);
                        var blog = response.Blog;
                        var newestPost = response.Result.Single();

                        if (newestPost.ShortUrl != tumblrChecker.LastPostShortUrl)
                        {
                            logger.LogTrace(LogString.From($"Found new Tumblr post for {tumblrChecker}: {newestPost.ShortUrl}."));
                            await channel.SendMessageAsync(embed: tumblrPostToEmbedMapper.ToEmbed(newestPost, blog));
                            await tumblrCheckerRepository.UpdateLastPostAsync(tumblrChecker, newestPost);
                        }
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, LogString.From($"Exception occurred when checking {tumblrChecker}."));
                    }

                    await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
                }

                await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
            }
        }
    }
}
