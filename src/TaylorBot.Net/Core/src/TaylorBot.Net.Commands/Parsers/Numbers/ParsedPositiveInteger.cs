using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Numbers
{
    public record ParsedPositiveInteger(int Value);

    public class PositiveIntegerParser : IOptionParser<ParsedPositiveInteger>
    {
        private readonly IntegerParser _integerParser;

        public PositiveIntegerParser(IntegerParser integerParser)
        {
            _integerParser = integerParser;
        }

        public async ValueTask<Result<ParsedPositiveInteger, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            var parsed = await _integerParser.ParseAsync(context, optionValue);

            if (parsed)
            {
                if (parsed.Value.Value <= 0)
                    return Error(new ParsingFailed("Number must be positive."));
                else
                    return new ParsedPositiveInteger(parsed.Value.Value);
            }
            else
            {
                return Error(parsed.Error);
            }
        }
    }
}
