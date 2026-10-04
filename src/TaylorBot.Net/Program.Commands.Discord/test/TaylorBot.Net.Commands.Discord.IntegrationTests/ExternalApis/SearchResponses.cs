using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;

public static class SearchResponses
{
    public static void ImgurUpload(this ExternalApi api) =>
        api.Json("POST", "https://api.imgur.com/3/image", new { success = true, data = new { link = "https://i.imgur.com/synthetic.png" } });

    public static void ImgurError(this ExternalApi api, string message) =>
        api.Json("POST", "https://api.imgur.com/3/image", new { data = new { error = message } }, HttpStatusCode.BadRequest);

    public static void UrbanDefinitions(this ExternalApi api) =>
        api.Json("GET", "https://api.urbandictionary.com/v0/define?term=swiftie", new
        {
            list = new[]
            {
                new { word = "swiftie", definition = "A fan of Taylor Swift.", author = "Alice", written_on = "2024-01-01T00:00:00Z", permalink = "https://example.invalid/first", thumbs_up = 13, thumbs_down = 2 },
                new { word = "swiftie", definition = "Someone who knows every bridge.", author = "Bob", written_on = "2024-01-02T00:00:00Z", permalink = "https://example.invalid/second", thumbs_up = 42, thumbs_down = 1 },
            },
        });

    public static void UrbanEmpty(this ExternalApi api) =>
        api.Json("GET", "https://api.urbandictionary.com/v0/define?term=swiftie", new { list = Array.Empty<object>() });

    public static void WolframAnswer(this ExternalApi api, bool understood = true)
    {
        JsonObject result = new() { ["success"] = understood, ["error"] = false, ["numpods"] = understood ? 2 : 0 };
        if (understood)
        {
            result["pods"] = JsonSerializer.SerializeToNode(new[]
            {
                new { subpods = new[] { new { plaintext = "2 + 2", img = new { src = "https://example.invalid/input.png" } } } },
                new { subpods = new[] { new { plaintext = "4", img = new { src = "https://example.invalid/result.png" } } } },
            });
        }

        api.Json("GET", "https://api.wolframalpha.com/v2/query?input=two%20plus%20two&appid=synthetic&output=json&ip=192.168.1.1&podindex=1,2",
            new JsonObject { ["queryresult"] = result });
    }

    public static void YouTubeResults(this ExternalApi api, params string[] videoIds) =>
        api.Json("GET", "https://youtube.googleapis.com/youtube/v3/search?part=snippet&q=Taylor&type=video&key=synthetic", new
        {
            kind = "youtube#searchListResponse",
            items = videoIds.Select(id => new { id = new { kind = "youtube#video", videoId = id }, snippet = new { title = "Taylor live" } }).ToArray(),
        });
}
