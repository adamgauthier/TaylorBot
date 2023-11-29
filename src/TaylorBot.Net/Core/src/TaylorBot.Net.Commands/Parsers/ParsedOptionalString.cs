using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.Commands.Parsers;

public record ParsedOptionalString(string? Value);

public class OptionalStringParser : IOptionParser<ParsedOptionalString>
{
    public ValueTask<Result<ParsedOptionalString, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        return new(new ParsedOptionalString(optionValue.HasValue ? optionValue.Value.GetString() : null));
    }
}
