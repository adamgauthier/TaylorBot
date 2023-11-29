using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Numbers;

public record ParsedOptionalInteger(int? Value);

public class OptionalIntegerParser : IOptionParser<ParsedOptionalInteger>
{
    public ValueTask<Result<ParsedOptionalInteger, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        return new(Parse(optionValue));
    }

    private static Result<ParsedOptionalInteger, ParsingFailed> Parse(JsonElement? optionValue)
    {
        if (!optionValue.HasValue)
        {
            return new ParsedOptionalInteger(null);
        }

        if (optionValue.Value.TryGetInt32(out var integer))
        {
            return new ParsedOptionalInteger(integer);
        }
        else
        {
            return Error(new ParsingFailed("Invalid number."));
        }
    }
}
