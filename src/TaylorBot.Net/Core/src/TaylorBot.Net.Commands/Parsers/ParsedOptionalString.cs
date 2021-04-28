using System.Text.Json;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsedOptionalString(string? Value) : IParseResult;

    public class OptionalStringParser : IOptionParser<ParsedOptionalString>
    {
        public ValueTask<IParseResult> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            return new(new ParsedOptionalString(optionValue.HasValue ? optionValue.Value.GetString() : null));
        }
    }
}
