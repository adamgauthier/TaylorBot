using System.Net;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;

public static class SearchAndWeatherResponses
{
    private const string SearchUrl = "https://customsearch.googleapis.com/customsearch/v1?cx=synthetic&safe=high&num=10&searchType=image&q=Taylor%20Swift&key=synthetic";

    public static void ImageSearch(this ExternalApi api) =>
        api.Json("GET", SearchUrl, new
        {
            searchInformation = new { formattedTotalResults = "1", formattedSearchTime = "0.13" },
            items = new[] { new { title = "Taylor Swift", link = "https://images.example.invalid/taylor.jpg", fileFormat = "image/jpeg",
                image = new { contextLink = "https://example.invalid/taylor", thumbnailLink = "https://images.example.invalid/thumb.jpg" } } },
        });

    public static void ImageSearchError(this ExternalApi api, string reason) =>
        api.Json("GET", SearchUrl, new { error = new { code = 403, message = "Synthetic search failure", errors = new[] { new { reason, domain = "usageLimits", message = "Synthetic search failure" } } } },
            HttpStatusCode.Forbidden);

    public static void Weather(this ExternalApi api) =>
        api.Json("GET", "https://api.pirateweather.net/forecast/synthetic/46.8130816,-71.2074596?exclude=minutely,hourly,daily,alerts,flags&units=si",
            new { currently = new { time = 1767225600, summary = "Partly cloudy", icon = "partly-cloudy-day", temperature = 13.3, humidity = 0.61, windSpeed = 5.5 } });
}
