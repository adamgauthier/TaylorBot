using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Infrastructure;

public class YouTubeClient : IYouTubeClient
{
    private readonly ILogger<YouTubeClient> _logger;
    private readonly YouTubeService _youTubeService;

    public YouTubeClient(ILogger<YouTubeClient> logger, YouTubeService youTubeService)
    {
        _logger = logger;
        _youTubeService = youTubeService;
    }

    public async ValueTask<IYouTubeSearchResult> SearchAsync(string query)
    {
        var request = _youTubeService.Search.List("snippet");
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
            _logger.LogWarning(e, "Unhandled error in YouTube Search API");
            return new GenericError(e);
        }
    }
}
