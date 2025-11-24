using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.YoutubeNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.YoutubeNotifier.Domain.Options;

namespace TaylorBot.Net.YoutubeNotifier.Domain;

public partial class YoutubeNotifierService(
    ILogger<YoutubeNotifierService> logger,
    IOptionsMonitor<YoutubeNotifierOptions> optionsMonitor,
    IYoutubeCheckerRepository youtubeCheckerRepository,
    YouTubeService youtubeService,
    YoutubePostToEmbedMapper youtubePostToEmbedMapper,
    Lazy<ITaylorBotClient> taylorBotClient
    )
{
    public async Task StartCheckingYoutubesAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(30));

        while (true)
        {
            try
            {
                await CheckAllYoutubesAsync();
            }
            catch (Exception e)
            {
                LogUnhandledExceptionCheckingYoutubes(e);
            }
            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
        }
    }

    public async ValueTask CheckAllYoutubesAsync()
    {
        foreach (var youtubeChecker in await youtubeCheckerRepository.GetYoutubeCheckersAsync())
        {
            try
            {
                var channel = taylorBotClient.Value.ResolveRequiredGuild(youtubeChecker.GuildId).GetRequiredTextChannel(youtubeChecker.ChannelId);

                var request = youtubeService.PlaylistItems.List(part: "snippet");
                request.PlaylistId = youtubeChecker.PlaylistId;
                var response = await request.ExecuteAsync();
                var newestPost = response.Items.First().Snippet;

                LogCheckingIfYoutubePostIsNew(newestPost.ResourceId.VideoId, youtubeChecker);

                if (youtubeChecker.LastVideoId == null || (
                    newestPost.ResourceId.VideoId != youtubeChecker.LastVideoId &&
                    (!newestPost.PublishedAtDateTimeOffset.HasValue || !youtubeChecker.LastPublishedAt.HasValue ||
                    newestPost.PublishedAtDateTimeOffset.Value > youtubeChecker.LastPublishedAt.Value)
                ))
                {
                    LogFoundNewYoutubePost(youtubeChecker, newestPost.ResourceId.VideoId);
                    await channel.SendMessageAsync(embed: youtubePostToEmbedMapper.ToEmbed(newestPost));
                    await youtubeCheckerRepository.UpdateLastPostAsync(youtubeChecker, newestPost);
                }
            }
            catch (Exception exception)
            {
                LogExceptionCheckingYoutube(exception, youtubeChecker);
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Checking if Youtube post '{VideoId}' for {YoutubeChecker} is new.")]
    private partial void LogCheckingIfYoutubePostIsNew(string videoId, YoutubeChecker youtubeChecker);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Found new Youtube post for {YoutubeChecker}: '{VideoId}'.")]
    private partial void LogFoundNewYoutubePost(YoutubeChecker youtubeChecker, string videoId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred when checking {YoutubeChecker}.")]
    private partial void LogExceptionCheckingYoutube(Exception exception, YoutubeChecker youtubeChecker);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in " + nameof(CheckAllYoutubesAsync) + ".")]
    private partial void LogUnhandledExceptionCheckingYoutubes(Exception exception);
}
