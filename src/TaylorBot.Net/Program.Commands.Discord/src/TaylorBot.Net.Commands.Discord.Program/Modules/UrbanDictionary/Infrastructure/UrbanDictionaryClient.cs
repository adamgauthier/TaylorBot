using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Domain;
using TaylorBot.Net.Core.Http;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Infrastructure;

public sealed class UrbanDictionaryClient(ILogger<UrbanDictionaryClient> logger, HttpClient client) : IUrbanDictionaryClient
{
    public async ValueTask<IUrbanDictionaryResult> SearchAsync(string query)
    {
        var queryString = new[] { $"term={query}" };

        return await client.ReadJsonWithErrorLogging<UrbanDictionaryResponse, IUrbanDictionaryResult>(
            c => c.GetAsync($"https://api.urbandictionary.com/v0/define?{string.Join('&', queryString)}"),
            handleSuccessAsync: success => Task.FromResult(HandleSuccess(success)),
            handleErrorAsync: error => Task.FromResult(HandleError(error)),
            logger);
    }

    private IUrbanDictionaryResult HandleSuccess(HttpSuccess<UrbanDictionaryResponse> result)
    {
        var list = result.Parsed.list;

        return new UrbanDictionaryResult(list.Select(a => new UrbanDictionaryResult.SlangDefinition(
            Word: a.word,
            Definition: a.definition,
            Author: a.author,
            WrittenOn: a.written_on,
            Link: a.permalink,
            UpvoteCount: a.thumbs_up,
            DownvoteCount: a.thumbs_down
        )).ToList());
    }

    private IUrbanDictionaryResult HandleError(HttpError error)
    {
        return new GenericUrbanError();
    }

    private record UrbanDictionaryResponse(IReadOnlyList<UrbanDictionaryResponse.SlangDefinition> list)
    {
        public record SlangDefinition(
            string word,
            string definition,
            string author,
            DateTimeOffset written_on,
            string permalink,
            int thumbs_up,
            int thumbs_down
        );
    }
}
