using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Core.Http;
using TaylorBot.Net.Core.Infrastructure.Extensions;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Infrastructure;

public class GooglePlacesNewClient(ILogger<GooglePlacesNewClient> logger, IOptionsMonitor<GoogleOptions> options, HttpClient client, TimeProvider timeProvider) : ILocationClient
{
    public async ValueTask<ILocationResult> GetLocationAsync(string search)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, "https://places.googleapis.com/v1/places:searchText")
        {
            Content = JsonContent.Create(new
            {
                textQuery = search,
                pageSize = 1,
            }),
        };
        request.Headers.Add("X-Goog-Api-Key", options.CurrentValue.GoogleApiKey);
        request.Headers.Add("X-Goog-FieldMask", "places.displayName,places.formattedAddress,places.location,places.types");

        return await client.ReadJsonWithErrorLogging<PlaceResponse, ILocationResult>(
            c => c.SendAsync(request),
            handleSuccessAsync: HandleLocationSuccessAsync,
            handleErrorAsync: error => Task.FromResult(HandleLocationError(error)),
            logger);
    }

    private Task<ILocationResult> HandleLocationSuccessAsync(HttpSuccess<PlaceResponse> result)
    {
        var place = result.Parsed;

        if (place.places?.Count > 0)
        {
            var first = place.places[0];

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

            return Task.FromResult<ILocationResult>(new LocationFoundResult(new(
                $"{first.location.latitude}",
                $"{first.location.longitude}",
                first.formattedAddress ?? first.displayName?.text ?? "Unknown location",
                IsGeneral: isGeneral && !isPrecise)));
        }

        return Task.FromResult<ILocationResult>(new LocationNotFoundResult());
    }

    private ILocationResult HandleLocationError(HttpError error)
    {
        logger.LogWarning(error.Exception, "Error occurred while calling Google Places Text Search (New) API");
        return new LocationGenericErrorResult();
    }

    private sealed record PlaceResponse(IReadOnlyList<PlaceResponse.Place>? places)
    {
        public sealed record Place(string? formattedAddress, PlaceDisplayName? displayName, PlaceLocation location, IReadOnlyList<string> types);
        public sealed record PlaceDisplayName(string text);
        public sealed record PlaceLocation(double latitude, double longitude);
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
