﻿using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsedString(string Value);

    public class StringParser : IOptionParser<ParsedString>
    {
        public ValueTask<Result<ParsedString, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (!optionValue.HasValue)
            {
                return new(Error(new ParsingFailed("String option is required.")));
            }

            return new(new ParsedString(optionValue.Value.GetString()!));
        }
    }
}