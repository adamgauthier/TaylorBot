using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Core.Http;
using TaylorBot.Net.Core.Infrastructure.Extensions;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Infrastructure;

public class GooglePlacesClient(ILogger<GooglePlacesClient> logger, IOptionsMonitor<GoogleOptions> options, HttpClient client, TimeProvider timeProvider) : ILocationClient
{
    public async ValueTask<ILocationResult> GetLocationAsync(string search)
    {
        var qs = new Dictionary<string, string>() {
            { "key", options.CurrentValue.GoogleApiKey },
            { "inputtype", "textquery" },
            { "fields", "formatted_address,geometry/location,type" },
            { "input", search },
        }.ToUrlQueryString();

        return await client.ReadJsonWithErrorLogging<PlaceResponse, ILocationResult>(
            c => c.GetAsync($"https://maps.googleapis.com/maps/api/place/findplacefromtext/json?{qs}"),
            handleSuccessAsync: HandleLocationSuccessAsync,
            handleErrorAsync: error => Task.FromResult(HandleLocationError(error)),
            logger);
    }

    private async Task<ILocationResult> HandleLocationSuccessAsync(HttpSuccess<PlaceResponse> result)
    {
        var place = result.Parsed;
        switch (place.status)
        {
            case "OK":
                var first = place.candidates[0];

                var isGeneral = first.types.Contains("country")
                    || first.types.Contains("political")
                    || first.types.Contains("locality")
                    || first.types.Contains("neighborhood")
                    || first.types.Contains("postal_town")
                    || first.types.Contains("archipelago")
                    || first.types.Contains("continent")
                    || first.types.Contains("colloquial_area")
                    || first.types.Any(t => t.Contains("administrative_area", StringComparison.InvariantCulture))
                    || first.types.Any(t => t.Contains("sublocality", StringComparison.InvariantCulture));

                var isPrecise = first.types.Contains("street_address")
                    || first.types.Contains("premise")
                    || first.types.Contains("subpremise");

                return new LocationFoundResult(new(
                    $"{first.geometry.location.lat}",
                    $"{first.geometry.location.lng}",
                    first.formatted_address,
                    IsGeneral: isGeneral && !isPrecise));

            case "ZERO_RESULTS":
                return new LocationNotFoundResult();

            default:
                await result.Response.LogContentAsync(logger, LogLevel.Warning, $"Unexpected status {place.status}");
                return new LocationGenericErrorResult();
        }
    }

    private ILocationResult HandleLocationError(HttpError error)
    {
        return new LocationGenericErrorResult();
    }

    private sealed record PlaceResponse(string status, IReadOnlyList<PlaceResponse.PlaceCandidate> candidates)
    {
        public sealed record PlaceCandidate(string formatted_address, PlaceGeometry geometry, IReadOnlyList<string> types);
        public sealed record PlaceGeometry(PlaceLocation location);
        public sealed record PlaceLocation(double lat, double lng);
    }

    public async ValueTask<ITimeZoneResult> GetTimeZoneForLocationAsync(string latitude, string longitude)
    {
        var qs = new Dictionary<string, string>() {
            { "key", options.CurrentValue.GoogleApiKey },
            { "timestamp", $"{timeProvider.GetUtcNow().ToUnixTimeSeconds()}" },
            { "location", $"{latitude},{longitude}" },
        }.ToUrlQueryString();

        return await client.ReadJsonWithErrorLogging<TimeZoneResponse, ITimeZoneResult>(
            c => c.GetAsync($"https://maps.googleapis.com/maps/api/timezone/json?{qs}"),
            handleSuccessAsync: HandleTimeZoneSuccessAsync,
            handleErrorAsync: error => Task.FromResult(HandleTimeZoneError(error)),
            logger);
    }

    private async Task<ITimeZoneResult> HandleTimeZoneSuccessAsync(HttpSuccess<TimeZoneResponse> result)
    {
        var timeZone = result.Parsed;
        switch (timeZone.status)
        {
            case "OK":
                return new TimeZoneResult(timeZone.timeZoneId);

            default:
                await result.Response.LogContentAsync(logger, LogLevel.Warning, $"Unexpected status {timeZone.status}");
                return new TimeZoneGenericErrorResult();
        }
    }

    private ITimeZoneResult HandleTimeZoneError(HttpError error)
    {
        return new TimeZoneGenericErrorResult();
    }

    private sealed record TimeZoneResponse(string status, string timeZoneId);
}
