using static TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Domain.UrbanDictionaryResult;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Domain;

public interface IUrbanDictionaryResult { }
public record UrbanDictionaryResult(IReadOnlyList<SlangDefinition> Definitions) : IUrbanDictionaryResult
{
    public record SlangDefinition(
        string Word,
        string Definition,
        string Author,
        DateTimeOffset WrittenOn,
        string Link,
        int UpvoteCount,
        int DownvoteCount);
};
public record GenericUrbanError() : IUrbanDictionaryResult;

public interface IUrbanDictionaryClient
{
    ValueTask<IUrbanDictionaryResult> SearchAsync(string query);
}
