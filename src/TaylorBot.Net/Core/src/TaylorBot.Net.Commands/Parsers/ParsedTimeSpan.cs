using OperationResult;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsedTimeSpan(TimeSpan Value);

    public class TimeSpanParser : IOptionParser<ParsedTimeSpan>
    {
        private const string UnrecognizedFormatText = "Unrecognized format. Please use one of these: **3m** = 3 minutes, **4h** = 4 hours, **5d** = 5 days.";

        public ValueTask<Result<ParsedTimeSpan, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (!optionValue.HasValue)
            {
                return new(Error(new ParsingFailed("Time option is required.")));
            }

            var input = optionValue.Value.GetString()!;

            var rawQuantity = input[0..^1];
            if (!uint.TryParse(rawQuantity, out var quantity))
            {
                return new(Error(new ParsingFailed(UnrecognizedFormatText)));
            }

            var suffix = char.ToLowerInvariant(input[^1]);

            return suffix switch
            {
                'm' => new(new ParsedTimeSpan(TimeSpan.FromMinutes(quantity))),
                'h' => new(new ParsedTimeSpan(TimeSpan.FromHours(quantity))),
                'd' => new(new ParsedTimeSpan(TimeSpan.FromDays(quantity))),
                _ => new(Error(new ParsingFailed(UnrecognizedFormatText))),
            };
        }
    }
}
