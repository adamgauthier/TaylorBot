using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Net;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Core.Http;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Infrastructure;

public partial class GaneshaSpeaksHoroscopeClient(ILogger<GaneshaSpeaksHoroscopeClient> logger, HttpClient client) : IHoroscopeClient
{
    public async ValueTask<IHoroscopeResult> GetHoroscopeAsync(string zodiacSign)
    {
        var result = await client.ReadStringWithErrorLogging(
            c => c.GetAsync($"https://www.ganeshaspeaks.com/horoscopes/daily-horoscope/{zodiacSign}/"),
            logger);

        if (result.IsSuccess)
        {
            HtmlDocument htmlDocument = new();

            try
            {
                htmlDocument.LoadHtml(result.Value);

                var horoscopeContent = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='horoscope-content']");
                ArgumentNullException.ThrowIfNull(horoscopeContent);

                var pElements = horoscopeContent.SelectNodes(".//p[not(ancestor::div[@class='horoscope-date'])]");
                ArgumentNullException.ThrowIfNull(pElements);

                var horoscope = string.Join("\n\n", pElements.Select(p => p.InnerText.Trim()));
                return new Horoscope(WebUtility.HtmlDecode(horoscope));
            }
            catch (Exception e)
            {
                LogUnableToParseHoroscope(e, result.Value);
                throw;
            }
        }
        else
        {
            return new GaneshaSpeaksGenericErrorResult();
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unable to parse horoscope: {Content}")]
    private partial void LogUnableToParseHoroscope(Exception exception, string content);
}
