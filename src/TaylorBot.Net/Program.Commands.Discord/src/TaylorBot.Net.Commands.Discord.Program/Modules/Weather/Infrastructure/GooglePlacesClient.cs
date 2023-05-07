using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Weather.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Weather.Infrastructure;

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
        var qs = string.Join('&', new[] {
            ("key", _options.CurrentValue.GoogleApiKey),
            ("inputtype", "textquery"),
            ("fields", "formatted_address,geometry/location"),
            ("input", search),
        }.Select(param => (UrlEncoder.Default.Encode(param.Item1), UrlEncoder.Default.Encode(param.Item2))));

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
}
