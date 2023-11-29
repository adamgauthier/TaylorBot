using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Core.Infrastructure.Extensions;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Infrastructure;

public class GooglePlacesClient : ILocationClient
{
    private readonly ILogger<GooglePlacesClient> _logger;
    private readonly IOptionsMonitor<GoogleOptions> _options;
    private readonly HttpClient _httpClient;

    public GooglePlacesClient(ILogger<GooglePlacesClient> logger, IOptionsMonitor<GoogleOptions> options, HttpClient httpClient)
    {
        _logger = logger;
        _options = options;
        _httpClient = httpClient;
    }

    private record PlaceResponse(string status, IReadOnlyList<PlaceResponse.PlaceCandidate> candidates)
    {
        public record PlaceCandidate(string formatted_address, PlaceGeometry geometry);
        public record PlaceGeometry(PlaceLocation location);
        public record PlaceLocation(double lat, double lng);
    }

    public async ValueTask<ILocationResult> GetLocationAsync(string search)
    {
        var qs = new Dictionary<string, string>() {
            { "key", _options.CurrentValue.GoogleApiKey },
            { "inputtype", "textquery" },
            { "fields", "formatted_address,geometry/location" },
            { "input", search },
        }.ToUrlQueryString();

        try
        {
            var response = await _httpClient.GetAsync($"https://maps.googleapis.com/maps/api/place/findplacefromtext/json?{qs}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var place = JsonSerializer.Deserialize<PlaceResponse>(json);

                switch (place?.status)
                {
                    case "OK":
                        var first = place.candidates[0];
                        return new LocationFoundResult(new($"{first.geometry.location.lat}", $"{first.geometry.location.lng}", first.formatted_address));

                    case "ZERO_RESULTS":
                        return new LocationNotFoundResult();

                    default:
                        _logger.LogWarning("Unexpected response from Google Places API: {json}", json.Replace("\n", " "));
                        return new LocationGenericErrorResult();
                }
            }
            else
            {
                _logger.LogWarning("Unexpected status code from Google Places API: {code}", response.StatusCode);
                return new LocationGenericErrorResult();
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unexpected exception when querying Google Places API");
            return new LocationGenericErrorResult();
        }
    }

    private record TimeZoneResponse(string status, string timeZoneId);

    public async ValueTask<ITimeZoneResult> GetTimeZoneForLocationAsync(string latitude, string longitude)
    {
        var qs = new Dictionary<string, string>() {
            { "key", _options.CurrentValue.GoogleApiKey },
            { "timestamp", $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}" },
            { "location", $"{latitude},{longitude}" },
        }.ToUrlQueryString();

        try
        {
            var response = await _httpClient.GetAsync($"https://maps.googleapis.com/maps/api/timezone/json?{qs}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var timeZone = JsonSerializer.Deserialize<TimeZoneResponse>(json);

                switch (timeZone?.status)
                {
                    case "OK":
                        return new TimeZoneResult(timeZone.timeZoneId);

                    default:
                        _logger.LogWarning("Unexpected response from Google Maps API: {json}", json.Replace("\n", " "));
                        return new TimeZoneGenericErrorResult();
                }
            }
            else
            {
                _logger.LogWarning("Unexpected status code from Google Maps API: {code}", response.StatusCode);
                return new TimeZoneGenericErrorResult();
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unexpected exception when querying Google Maps API");
            return new TimeZoneGenericErrorResult();
        }
    }
}
