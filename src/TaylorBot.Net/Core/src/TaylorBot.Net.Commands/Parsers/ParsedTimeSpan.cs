using OperationResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsedTimeSpan(TimeSpan Value);

    public class TimeSpanParser : IOptionParser<ParsedTimeSpan>
    {
        private const string UnrecognizedFormatText = "Unrecognized format. Examples: **3m** = 3 minutes, **4h** = 4 hours, **5d** = 5 days, **1d 3h** = 1 day and 3 hours.";

        public ValueTask<Result<ParsedTimeSpan, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (!optionValue.HasValue)
            {
                return new(Error(new ParsingFailed("Time option is required.")));
            }

            var input = optionValue.Value.GetString()!;

            var components = input.Split(' ');

            if (components.Length > 3)
            {
                return new(Error(new ParsingFailed(UnrecognizedFormatText)));
            }

            var parsedComponents = new Dictionary<char, TimeSpan>();

            foreach (var component in components)
            {
                var rawQuantity = component[0..^1];
                if (!uint.TryParse(rawQuantity, out var quantity))
                {
                    return new(Error(new ParsingFailed(UnrecognizedFormatText)));
                }

                var suffix = char.ToLowerInvariant(component[^1]);
                var parsed = ParseTimeSpan(quantity, suffix);
                if (!parsed.HasValue || parsedComponents.ContainsKey(suffix))
                {
                    return new(Error(new ParsingFailed(UnrecognizedFormatText)));
                }

                parsedComponents.Add(suffix, parsed.Value);
            }

            var sum = parsedComponents.Values.Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
            return new(new ParsedTimeSpan(sum));
        }

        private static TimeSpan? ParseTimeSpan(uint quantity, char suffix)
        {
            return suffix switch
            {
                'm' => TimeSpan.FromMinutes(quantity),
                'h' => TimeSpan.FromHours(quantity),
                'd' => TimeSpan.FromDays(quantity),
                _ => null,
            };
        }
    }
}
