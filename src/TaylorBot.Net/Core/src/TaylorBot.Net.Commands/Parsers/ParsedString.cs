using System.Text.Json;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsedString(string Value) : IParseResult;

    public class StringParser : IOptionParser<ParsedString>
    {
        public ValueTask<IParseResult> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (!optionValue.HasValue)
            {
                return new(new ParsingFailed("String option is required."));
            }

            return new(new ParsedString(optionValue.Value.GetString()!));
        }
    }
}
