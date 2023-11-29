using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.TumblrNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.TumblrNotifier.Domain.Options;

namespace TaylorBot.Net.TumblrNotifier.Domain
{
    public class TumblrNotifierService
    {
        private readonly ILogger<TumblrNotifierService> _logger;
        private readonly IOptionsMonitor<TumblrNotifierOptions> _optionsMonitor;
        private readonly ITumblrCheckerRepository _tumblrCheckerRepository;
        private readonly TumblrClient _tumblrClient;
        private readonly TumblrPostToEmbedMapper _tumblrPostToEmbedMapper;
        private readonly Lazy<ITaylorBotClient> _taylorBotClient;

        public TumblrNotifierService(
            ILogger<TumblrNotifierService> logger,
            IOptionsMonitor<TumblrNotifierOptions> optionsMonitor,
            ITumblrCheckerRepository tumblrCheckerRepository,
            TumblrClient tumblrClient,
            TumblrPostToEmbedMapper tumblrPostToEmbedMapper,
            Lazy<ITaylorBotClient> taylorBotClient
        )
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _tumblrCheckerRepository = tumblrCheckerRepository;
            _tumblrClient = tumblrClient;
            _tumblrPostToEmbedMapper = tumblrPostToEmbedMapper;
            _taylorBotClient = taylorBotClient;
        }

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
                    _logger.LogError(e, $"Unhandled exception in {nameof(CheckAllTumblrsAsync)}.");
                }
                await Task.Delay(_optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
            }
        }

        public async ValueTask CheckAllTumblrsAsync()
        {
            foreach (var tumblrChecker in await _tumblrCheckerRepository.GetTumblrCheckersAsync())
            {
                try
                {
                    var channel = _taylorBotClient.Value.ResolveRequiredGuild(tumblrChecker.GuildId).GetRequiredTextChannel(tumblrChecker.ChannelId);

                    var response = await _tumblrClient.GetPostsAsync(blogName: tumblrChecker.BlogName, filter: PostFilter.Text, count: 1);
                    var blog = response.Blog;
                    var newestPost = response.Result.Single();

                    if (newestPost.ShortUrl != tumblrChecker.LastPostShortUrl)
                    {
                        _logger.LogDebug($"Found new Tumblr post for {tumblrChecker}: {newestPost.ShortUrl}.");
                        await channel.SendMessageAsync(embed: _tumblrPostToEmbedMapper.ToEmbed(newestPost, blog));
                        await _tumblrCheckerRepository.UpdateLastPostAsync(tumblrChecker, newestPost);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"Exception occurred when checking {tumblrChecker}.");
                }

                await Task.Delay(_optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
            }
        }
    }
}
