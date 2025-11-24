using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Infrastructure;

public partial class YouTubeClient(ILogger<YouTubeClient> logger, YouTubeService youTubeService) : IYouTubeClient
{
    public async ValueTask<IYouTubeSearchResult> SearchAsync(string query)
    {
        var request = youTubeService.Search.List("snippet");
        request.Type = "video";
        request.Q = query;

        try
        {
            var response = await request.ExecuteAsync();

            var videoUrls = response.Items.Select(i => $"https://youtu.be/{i.Id.VideoId}").ToList();

            return new SuccessfulSearch(videoUrls);
        }
        catch (Exception e)
        {
            LogUnhandledErrorInYouTubeSearch(e);
            return new GenericError(e);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unhandled error in YouTube Search API")]
    private partial void LogUnhandledErrorInYouTubeSearch(Exception exception);
}
