using System.Net;
using System.Text;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Birthday;

public static class HoroscopeResponses
{
    private const string Url = "https://www.ganeshaspeaks.com/horoscopes/daily-horoscope/Sagittarius/";

    public static void Horoscope(this ExternalApi api) =>
        api.Bytes(Url, Encoding.UTF8.GetBytes("""
            <div class="horoscope-content">
              <div class="horoscope-date"><p>Not horoscope content</p></div>
              <p>Make time for friends &amp; music.</p><p>Try something new.</p>
            </div>
            """), "text/html");

    public static void HoroscopeUnavailable(this ExternalApi api) =>
        api.Json("GET", Url, new { error = "Unavailable" }, HttpStatusCode.ServiceUnavailable);
}
