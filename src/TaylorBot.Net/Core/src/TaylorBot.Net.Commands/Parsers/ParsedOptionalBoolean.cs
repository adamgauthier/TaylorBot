using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.Commands.Parsers;

public record ParsedOptionalBoolean(bool? Value);

public class OptionalBooleanParser : IOptionParser<ParsedOptionalBoolean>
{
    public ValueTask<Result<ParsedOptionalBoolean, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        return new(new ParsedOptionalBoolean(optionValue.HasValue ? optionValue.Value.GetBoolean() : null));
    }
}
