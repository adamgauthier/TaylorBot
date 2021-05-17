﻿using OperationResult;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsingFailed(string Message) : ICommandResult;

    public interface IOptionParser
    {
        Type OptionType { get; }
        ValueTask<Result<object?, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue);
    }

    public interface IOptionParser<T> : IOptionParser
    {
        Type IOptionParser.OptionType => typeof(T);
        async ValueTask<Result<object?, ParsingFailed>> IOptionParser.ParseAsync(RunContext context, JsonElement? optionValue)
        {
            var result = await ParseAsync(context, optionValue);
            if (result)
                return result.Value;
            else
                return Error(result.Error);
        }

        new ValueTask<Result<T, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue);
    }

    public record NoOptions();
}
