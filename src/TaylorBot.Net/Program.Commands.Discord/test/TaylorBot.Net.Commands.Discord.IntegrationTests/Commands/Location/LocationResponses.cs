using System.Net;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Location;

public static class LocationResponses
{
    public static void Place(this ExternalApi api, string type = "locality") =>
        api.Json("POST", "https://places.googleapis.com/v1/places:searchText", new
        {
            places = new[]
            {
                new
                {
                    formattedAddress = "Quebec City, QC, Canada",
                    location = new { latitude = 46.8130816, longitude = -71.2074596 },
                    types = new[] { type },
                },
            },
        });

    public static void PlaceNotFound(this ExternalApi api) =>
        api.Json("POST", "https://places.googleapis.com/v1/places:searchText", new { places = Array.Empty<object>() });

    public static void PlaceUnavailable(this ExternalApi api) =>
        api.Json("POST", "https://places.googleapis.com/v1/places:searchText", new { error = "Unavailable" }, HttpStatusCode.ServiceUnavailable);

    public static void PlaceTimeZone(this ExternalApi api, string status = "OK") =>
        api.Json("GET", "https://maps.googleapis.com/maps/api/timezone/json?key=synthetic&timestamp=1&location=46.8130816,-71.2074596",
            new { status, timeZoneId = "America/Toronto" }, numericQueryParameter: "timestamp");

    public static void ForecastUnavailable(this ExternalApi api) =>
        api.Json("GET", "https://api.pirateweather.net/forecast/synthetic/46.8130816,-71.2074596?exclude=minutely,hourly,daily,alerts,flags&units=si",
            new { error = "Unavailable" }, HttpStatusCode.ServiceUnavailable);
}
