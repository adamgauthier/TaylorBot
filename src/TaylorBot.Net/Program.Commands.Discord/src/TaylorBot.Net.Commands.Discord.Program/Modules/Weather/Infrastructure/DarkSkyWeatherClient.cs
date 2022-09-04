using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Weather.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Weather.Infrastructure
{
    public class DarkSkyWeatherClient : IWeatherClient
    {
        private static readonly string ForecastQueryString = QueryString.Create(new[] {
            new KeyValuePair<string, string>("exclude", "minutely,hourly,daily,alerts,flags"),
            new KeyValuePair<string, string>("units", "si"),
        }).ToUriComponent();

        private readonly ILogger<DarkSkyWeatherClient> _logger;
        private readonly IOptionsMonitor<WeatherOptions> _options;
        private readonly HttpClient _httpClient;

        public DarkSkyWeatherClient(ILogger<DarkSkyWeatherClient> logger, IOptionsMonitor<WeatherOptions> options, HttpClient httpClient)
        {
            _logger = logger;
            _options = options;
            _httpClient = httpClient;
        }

        private record ForecastApi(ForecastApi.Currently currently)
        {
            public record Currently(long time, string summary, string icon, double temperature, double humidity, double windSpeed);
        }

        public async ValueTask<IForecastResult> GetCurrentForecastAsync(string latitude, string longitude)
        {
            var response = await _httpClient.GetAsync($"https://api.darksky.net/forecast/{_options.CurrentValue.DarkSkyApiKey}/{latitude},{longitude}{ForecastQueryString}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var forecast = JsonSerializer.Deserialize<ForecastApi>(json)!;

                return new CurrentForecast(
                    Summary: forecast.currently.summary,
                    TemperatureCelsius: forecast.currently.temperature,
                    WindSpeed: forecast.currently.windSpeed,
                    Humidity: forecast.currently.humidity,
                    Time: forecast.currently.time,
                    IconUrl: $"https://darksky.net/images/weather-icons/{forecast.currently.icon}.png"
                );
            }
            else
            {
                _logger.LogWarning("Unexpected status code from Dark Sky API: {code}", response.StatusCode);
                return new WeatherGenericErrorResult();
            }
        }
    }
}
