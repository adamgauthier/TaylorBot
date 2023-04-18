using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Numbers;

public record ParsedInteger(int Value);

public class IntegerParser : IOptionParser<ParsedInteger>
{
    public ValueTask<Result<ParsedInteger, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        return new(Parse(optionValue));
    }

    private static Result<ParsedInteger, ParsingFailed> Parse(JsonElement? optionValue)
    {
        if (!optionValue.HasValue)
        {
            return Error(new ParsingFailed("Number option is required."));
        }

        if (optionValue.Value.TryGetInt32(out var integer))
        {
            return new ParsedInteger(integer);
        }
        else
        {
            return Error(new ParsingFailed("Invalid number."));
        }
    }
}
