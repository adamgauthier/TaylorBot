using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Knowledge.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Knowledge.Infrastructure
{
    public class GaneshaSpeaksHoroscopeClient : IHoroscopeClient
    {
        private static readonly Regex DateRegex = new("<p class=\"mb-0\">(.*?)<\\/p>", RegexOptions.Singleline);
        private static readonly Regex HoroscopeRegex = new("<p id=\"horo_content\">(.*)<\\/p>");

        private readonly ILogger<GaneshaSpeaksHoroscopeClient> _logger;
        private readonly HttpClient _httpClient;

        public GaneshaSpeaksHoroscopeClient(ILogger<GaneshaSpeaksHoroscopeClient> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async ValueTask<IHoroscopeResult> GetHoroscopeAsync(string zodiacSign)
        {
            var response = await _httpClient.GetAsync($"https://www.ganeshaspeaks.com/horoscopes/daily-horoscope/{zodiacSign}/");

            if (response.IsSuccessStatusCode)
            {
                var responseAsString = await response.Content.ReadAsStringAsync();

                var dateMatch = DateRegex.Match(responseAsString);

                if (!dateMatch.Success)
                {
                    _logger.LogWarning("Unable to parse date from GaneshaSpeaks response.");
                    return new GaneshaSpeaksGenericErrorResult("DateParsingFailed");
                }

                var horoscopeMatch = HoroscopeRegex.Match(responseAsString);

                if (!horoscopeMatch.Success)
                {
                    return new HoroscopeUnavailable();
                }

                var dateString = dateMatch.Groups[1].Value.Trim();
                var date = new DateOnly(
                    day: int.Parse(dateString[0..2]),
                    month: int.Parse(dateString[3..5]),
                    year: int.Parse(dateString[6..10])
                );

                return new Horoscope(horoscopeMatch.Groups[1].Value, date);
            }
            else
            {
                _logger.LogWarning("Unexpected status code when fetching from GaneshaSpeaks ({StatusCode}).", response.StatusCode);
                return new GaneshaSpeaksGenericErrorResult(response.StatusCode.ToString());
            }
        }
    }
}
