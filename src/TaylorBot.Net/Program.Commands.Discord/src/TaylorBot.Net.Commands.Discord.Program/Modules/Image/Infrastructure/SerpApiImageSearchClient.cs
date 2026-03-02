using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Core.Globalization;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Infrastructure;

public partial class SerpApiImageSearchClient(ILogger<SerpApiImageSearchClient> logger, IOptionsMonitor<SerpApiOptions> options, HttpClient httpClient) : IImageSearchClient
{
    private sealed record SerpApiResponse(
        [property: JsonPropertyName("search_metadata")] SerpApiSearchMetadata? SearchMetadata,
        [property: JsonPropertyName("images_results")] IReadOnlyList<SerpApiImage>? ImagesResults
    );

    private sealed record SerpApiSearchMetadata(
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("total_time_taken")] double? TotalTimeTaken
    );

    private sealed record SerpApiImage(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("original")] string Original,
        [property: JsonPropertyName("link")] string Link,
        [property: JsonPropertyName("thumbnail")] string? Thumbnail
    );

    public async ValueTask<IImageSearchResult> SearchImagesAsync(string query)
    {
        try
        {
            var url = $"https://serpapi.com/search.json?engine=google_images&q={Uri.EscapeDataString(query)}&num=10&api_key={options.CurrentValue.ApiKey}";

            var result = await httpClient.GetFromJsonAsync<SerpApiResponse>(url);

            var images = result?.ImagesResults?.Select(i => new ImageResult(
                Title: i.Title,
                PageUrl: i.Link,
                ImageUrl: i.Original
            )).ToList() ?? [];

            return new SuccessfulSearch(
                images,
                $"{images.Count}",
                result?.SearchMetadata?.TotalTimeTaken?.ToString("F2", TaylorBotCulture.Culture) ?? "N/A");
        }
        catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            return new DailyLimitExceeded();
        }
        catch (Exception e)
        {
            LogUnhandledErrorInSerpApiImageSearch(e);
            return new GenericError(e);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unhandled error in SerpApi Image Search API")]
    private partial void LogUnhandledErrorInSerpApiImageSearch(Exception exception);
}
