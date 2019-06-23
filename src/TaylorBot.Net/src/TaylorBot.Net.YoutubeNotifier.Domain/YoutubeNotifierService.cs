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
        private readonly ILogger<YoutubeNotifierService> logger;
        private readonly IOptionsMonitor<YoutubeNotifierOptions> optionsMonitor;
        private readonly IYoutubeCheckerRepository youtubeCheckerRepository;
        private readonly YouTubeService youtubeService;
        private readonly YoutubePostToEmbedMapper youtubePostToEmbedMapper;
        private readonly TaylorBotClient taylorBotClient;

        public YoutubeNotifierService(
            ILogger<YoutubeNotifierService> logger,
            IOptionsMonitor<YoutubeNotifierOptions> optionsMonitor,
            IYoutubeCheckerRepository youtubeCheckerRepository,
            YouTubeService youtubeService,
            YoutubePostToEmbedMapper youtubePostToEmbedMapper,
            TaylorBotClient taylorBotClient)
        {
            this.logger = logger;
            this.optionsMonitor = optionsMonitor;
            this.youtubeCheckerRepository = youtubeCheckerRepository;
            this.youtubeService = youtubeService;
            this.youtubePostToEmbedMapper = youtubePostToEmbedMapper;
            this.taylorBotClient = taylorBotClient;
        }

        public async Task StartYoutubeCheckerAsync()
        {
            while (true)
            {
                foreach (var youtubeChecker in await youtubeCheckerRepository.GetYoutubeCheckersAsync())
                {
                    try
                    {
                        var channel = taylorBotClient.ResolveRequiredGuild(youtubeChecker.GuildId).GetRequiredTextChannel(youtubeChecker.ChannelId);

                        var request = youtubeService.PlaylistItems.List(part: "snippet");
                        request.PlaylistId = youtubeChecker.PlaylistId;
                        var response = await request.ExecuteAsync();
                        var newestPost = response.Items.First().Snippet;

                        if (newestPost.ResourceId.VideoId != youtubeChecker.LastVideoId)
                        {
                            logger.LogTrace(LogString.From($"Found new Youtube post for {youtubeChecker}: {newestPost.ResourceId.VideoId}."));
                            await channel.SendMessageAsync(embed: youtubePostToEmbedMapper.ToEmbed(newestPost));
                            await youtubeCheckerRepository.UpdateLastPostAsync(youtubeChecker, newestPost);
                        }
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, LogString.From($"Exception occurred when checking {youtubeChecker}."));
                    }

                    await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
                }

                await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
            }
        }
    }
}
