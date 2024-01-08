using Google;
using Google.Apis.CustomSearchAPI.v1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Infrastructure;

public class GoogleCustomSearchClient(ILogger<GoogleCustomSearchClient> logger, IOptionsMonitor<ImageOptions> options, CustomSearchAPIService customSearchAPIService) : IImageSearchClient
{
    public async ValueTask<IImageSearchResult> SearchImagesAsync(string query)
    {
        var request = customSearchAPIService.Cse.List();
        request.Cx = options.CurrentValue.GoogleCustomSearchEngineId;
        request.Safe = CseResource.ListRequest.SafeEnum.High;
        request.Num = 10;
        request.SearchType = CseResource.ListRequest.SearchTypeEnum.Image;
        request.Q = query;

        try
        {
            var response = await request.ExecuteAsync();

            var results = response.Items?.Select(i => new ImageResult(
                Title: i.Title,
                PageUrl: i.Image.ContextLink,
                ImageUrl: i.FileFormat == "image/svg+xml" ? i.Image.ThumbnailLink : i.Link
            ))?.ToList() ?? [];

            return new SuccessfulSearch(results, response.SearchInformation.FormattedTotalResults, response.SearchInformation.FormattedSearchTime);
        }
        catch (GoogleApiException e) when (e.Error.Errors[0].Reason == "rateLimitExceeded")
        {
            return new DailyLimitExceeded();
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Unhandled error in Google Custom Search API");
            return new GenericError(e);
        }
    }
}
