using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Core.Http;
using TaylorBot.Net.Core.Infrastructure.Extensions;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Infrastructure;

public class PirateWeatherClient(ILogger<PirateWeatherClient> logger, IOptionsMonitor<WeatherOptions> options, HttpClient client) : IWeatherClient
{
    private static readonly string ForecastQueryString = new Dictionary<string, string>
    {
        { "exclude", "minutely,hourly,daily,alerts,flags" },
        { "units", "si" },
    }.ToUrlQueryString();

    public async ValueTask<IForecastResult> GetCurrentForecastAsync(string latitude, string longitude)
    {
        return await client.ReadJsonWithErrorLogging<ForecastApi, IForecastResult>(
            c => c.GetAsync($"https://api.pirateweather.net/forecast/{options.CurrentValue.PirateWeatherApiKey}/{latitude},{longitude}?{ForecastQueryString}"),
            handleSuccessAsync: success => Task.FromResult(HandleSuccess(success)),
            handleErrorAsync: error => Task.FromResult(HandleError(error)),
            logger);
    }

    private IForecastResult HandleSuccess(HttpSuccess<ForecastApi> result)
    {
        var forecast = result.Parsed;

        return new CurrentForecast(
            Summary: forecast.currently.summary,
            TemperatureCelsius: forecast.currently.temperature,
            WindSpeed: forecast.currently.windSpeed,
            Humidity: forecast.currently.humidity,
            Time: forecast.currently.time,
            IconUrl: GetIconUrl(forecast.currently.icon)
        );
    }

    private IForecastResult HandleError(HttpError error)
    {
        return new WeatherGenericErrorResult();
    }

    private static string? GetIconUrl(string iconName)
    {
        return iconName switch
        {
            "clear-day" => "https://i.imgur.com/juu8tLC.png",
            "clear-night" => "https://i.imgur.com/ewFmV1R.png",
            "cloudy" => "https://i.imgur.com/2t0W3Dp.png",
            "fog" => "https://i.imgur.com/0UFQXeg.png",
            "hail" => "https://i.imgur.com/2DxYDy4.png",
            "partly-cloudy-day" => "https://i.imgur.com/FsCVjeW.png",
            "partly-cloudy-night" => "https://i.imgur.com/BnqzZAz.png",
            "rain-snow-showers-day" => "https://i.imgur.com/HKnMjWw.png",
            "rain-snow-showers-night" => "https://i.imgur.com/iOGkURj.png",
            "rain-snow" => "https://i.imgur.com/RMA4ggh.png",
            "rain" => "https://i.imgur.com/vaemCtB.png",
            "showers-day" => "https://i.imgur.com/DuU8RLd.png",
            "showers-night" => "https://i.imgur.com/57vG5y0.png",
            "sleet" => "https://i.imgur.com/LpbCzrj.png",
            "snow-showers-day" => "https://i.imgur.com/TPJZaJv.png",
            "snow-showers-night" => "https://i.imgur.com/pXglbIS.png",
            "snow" => "https://i.imgur.com/owLREMK.png",
            "thunder-rain" => "https://i.imgur.com/kXZ42nI.png",
            "thunder-showers-day" => "https://i.imgur.com/hLlALgG.png",
            "thunder-showers-night" => "https://i.imgur.com/nFwIFYU.png",
            "thunder" => "https://i.imgur.com/1OAoSDA.png",
            "wind" => "https://i.imgur.com/W4VgX8l.png",
            _ => null,
        };
    }

    private sealed record ForecastApi(ForecastApi.Currently currently)
    {
        public sealed record Currently(long time, string summary, string icon, double temperature, double humidity, double windSpeed);
    }
}
