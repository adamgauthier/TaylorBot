using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.YoutubeNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.YoutubeNotifier.Domain.Options;

namespace TaylorBot.Net.YoutubeNotifier.Domain
{
    public class YoutubeNotifierService
    {
        private readonly ILogger<YoutubeNotifierService> _logger;
        private readonly IOptionsMonitor<YoutubeNotifierOptions> _optionsMonitor;
        private readonly IYoutubeCheckerRepository _youtubeCheckerRepository;
        private readonly YouTubeService _youtubeService;
        private readonly YoutubePostToEmbedMapper _youtubePostToEmbedMapper;
        private readonly ITaylorBotClient _taylorBotClient;

        public YoutubeNotifierService(
            ILogger<YoutubeNotifierService> logger,
            IOptionsMonitor<YoutubeNotifierOptions> optionsMonitor,
            IYoutubeCheckerRepository youtubeCheckerRepository,
            YouTubeService youtubeService,
            YoutubePostToEmbedMapper youtubePostToEmbedMapper,
            ITaylorBotClient taylorBotClient)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _youtubeCheckerRepository = youtubeCheckerRepository;
            _youtubeService = youtubeService;
            _youtubePostToEmbedMapper = youtubePostToEmbedMapper;
            _taylorBotClient = taylorBotClient;
        }

        public async Task StartCheckingYoutubesAsync()
        {
            while (true)
            {
                try
                {
                    await CheckAllYoutubesAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, LogString.From($"Unhandled exception in {nameof(CheckAllYoutubesAsync)}."));
                }
                await Task.Delay(_optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
            }
        }

        public async ValueTask CheckAllYoutubesAsync()
        {
            foreach (var youtubeChecker in await _youtubeCheckerRepository.GetYoutubeCheckersAsync())
            {
                try
                {
                    var channel = _taylorBotClient.ResolveRequiredGuild(youtubeChecker.GuildId).GetRequiredTextChannel(youtubeChecker.ChannelId);

                    var request = _youtubeService.PlaylistItems.List(part: "snippet");
                    request.PlaylistId = youtubeChecker.PlaylistId;
                    var response = await request.ExecuteAsync();
                    var newestPost = new ParsedPlaylistItemSnippet(response.Items.First().Snippet);

                    if (youtubeChecker.LastVideoId == null || (
                        newestPost.Snippet.ResourceId.VideoId != youtubeChecker.LastVideoId &&
                        (newestPost.PublishedAt == null || !youtubeChecker.LastPublishedAt.HasValue ||
                        newestPost.PublishedAt > youtubeChecker.LastPublishedAt.Value)
                    ))
                    {
                        _logger.LogTrace(LogString.From($"Found new Youtube post for {youtubeChecker}: {newestPost.Snippet.ResourceId.VideoId}."));
                        await channel.SendMessageAsync(embed: _youtubePostToEmbedMapper.ToEmbed(newestPost));
                        await _youtubeCheckerRepository.UpdateLastPostAsync(youtubeChecker, newestPost);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, LogString.From($"Exception occurred when checking {youtubeChecker}."));
                }

                await Task.Delay(_optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
            }
        }
    }
}
