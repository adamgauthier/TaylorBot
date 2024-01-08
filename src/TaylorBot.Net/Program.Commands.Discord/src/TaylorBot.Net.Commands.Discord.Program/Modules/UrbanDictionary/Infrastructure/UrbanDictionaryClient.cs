using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Infrastructure;

public class UrbanDictionaryClient(ILogger<UrbanDictionaryClient> logger) : IUrbanDictionaryClient
{
    private readonly HttpClient _httpClient = new();

    public async ValueTask<IUrbanDictionaryResult> SearchAsync(string query)
    {
        var queryString = new[] { $"term={query}" };
        try
        {
            var response = await _httpClient.GetAsync($"https://api.urbandictionary.com/v0/define?{string.Join('&', queryString)}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

                var list = jsonDocument.RootElement.GetProperty("list");

                return new UrbanDictionaryResult(list.EnumerateArray().Select(a => new UrbanDictionaryResult.SlangDefinition(
                    Word: a.GetProperty("word").GetString()!,
                    Definition: a.GetProperty("definition").GetString()!,
                    Author: a.GetProperty("author").GetString()!,
                    WrittenOn: a.GetProperty("written_on").GetDateTimeOffset(),
                    Link: a.GetProperty("permalink").GetString()!,
                    UpvoteCount: a.GetProperty("thumbs_up").GetInt32(),
                    DownvoteCount: a.GetProperty("thumbs_down").GetInt32()
                )).ToList());
            }
            else
            {
                try
                {
                    var body = await response.Content.ReadAsStringAsync();
                    logger.LogWarning("Error response from UrbanDictionary ({StatusCode}): {Body}", response.StatusCode, body);
                    return new GenericUrbanError();
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Unhandled error when parsing JSON in UrbanDictionary error response ({StatusCode}):", response.StatusCode);
                    return new GenericUrbanError();
                }
            }
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Unhandled error in UrbanDictionary API:");
            return new GenericUrbanError();
        }
    }
}
