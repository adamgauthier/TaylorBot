using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Infrastructure;

public class GaneshaSpeaksHoroscopeClient(ILogger<GaneshaSpeaksHoroscopeClient> logger, HttpClient httpClient) : IHoroscopeClient
{
    private static readonly Regex HoroscopeRegex = new("<p id=\"horo_content\">(.*)<\\/p>");

    public async ValueTask<IHoroscopeResult> GetHoroscopeAsync(string zodiacSign)
    {
        var response = await httpClient.GetAsync($"https://www.ganeshaspeaks.com/horoscopes/daily-horoscope/{zodiacSign}/");

        if (response.IsSuccessStatusCode)
        {
            var responseAsString = await response.Content.ReadAsStringAsync();

            var horoscopeMatch = HoroscopeRegex.Match(responseAsString);

            if (!horoscopeMatch.Success)
            {
                return new HoroscopeUnavailable();
            }

            return new Horoscope(horoscopeMatch.Groups[1].Value);
        }
        else
        {
            logger.LogWarning("Unexpected status code when fetching from GaneshaSpeaks ({StatusCode}).", response.StatusCode);
            return new GaneshaSpeaksGenericErrorResult(response.StatusCode.ToString());
        }
    }
}
