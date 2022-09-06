using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsedOptionalBoolean(bool? Value);

    public class OptionalBooleanParser : IOptionParser<ParsedOptionalBoolean>
    {
        public ValueTask<Result<ParsedOptionalBoolean, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            return new(new ParsedOptionalBoolean(optionValue.HasValue ? optionValue.Value.GetBoolean() : null));
        }
    }
}
