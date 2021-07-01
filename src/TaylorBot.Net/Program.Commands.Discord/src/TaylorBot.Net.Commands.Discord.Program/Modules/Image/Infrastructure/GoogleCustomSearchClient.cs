using Google;
using Google.Apis.CustomSearchAPI.v1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Infrastructure
{
    public class GoogleCustomSearchClient : IImageSearchClient
    {
        private readonly ILogger<GoogleCustomSearchClient> _logger;
        private readonly IOptionsMonitor<ImageOptions> _options;
        private readonly CustomSearchAPIService _customSearchAPIService;

        public GoogleCustomSearchClient(ILogger<GoogleCustomSearchClient> logger, IOptionsMonitor<ImageOptions> options, CustomSearchAPIService customSearchAPIService)
        {
            _logger = logger;
            _options = options;
            _customSearchAPIService = customSearchAPIService;
        }

        public async ValueTask<IImageSearchResult> SearchImagesAsync(string query)
        {
            var request = _customSearchAPIService.Cse.List();
            request.Cx = _options.CurrentValue.GoogleCustomSearchEngineId;
            request.Safe = CseResource.ListRequest.SafeEnum.High;
            request.Num = 10;
            request.SearchType = CseResource.ListRequest.SearchTypeEnum.Image;
            request.Q = query;

            try
            {
                var response = await request.ExecuteAsync();

                var results = response.Items.Select(i => new ImageResult(
                    Title: i.Title,
                    PageUrl: i.Image.ContextLink,
                    ImageUrl: i.FileFormat == "image/svg+xml" ? i.Image.ThumbnailLink : i.Link
                )).ToList();

                return new SuccessfulSearch(results, response.SearchInformation.FormattedTotalResults, response.SearchInformation.FormattedSearchTime);
            }
            catch (GoogleApiException e) when (e.Error.Errors[0].Reason == "rateLimitExceeded")
            {
                return new DailyLimitExceeded();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unhandled error in Google Custom Search API");
                return new GenericError(e);
            }
        }
    }
}
