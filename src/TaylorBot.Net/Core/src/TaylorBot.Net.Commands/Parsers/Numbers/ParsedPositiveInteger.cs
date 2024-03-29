﻿using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Numbers;

public record ParsedPositiveInteger(int Value);

public class PositiveIntegerParser(IntegerParser integerParser) : IOptionParser<ParsedPositiveInteger>
{
    public async ValueTask<Result<ParsedPositiveInteger, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsed = await integerParser.ParseAsync(context, optionValue, resolved);

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
