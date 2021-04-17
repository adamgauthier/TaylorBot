using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Parsers
{
    public interface IParseResult : ICommandResult { }
    public record ParsingFailed(string Message) : IParseResult;

    public interface IOptionParser
    {
        Type OptionType { get; }
        ValueTask<IParseResult> ParseAsync(RunContext context, JsonElement? optionValue);
    }

    public interface IOptionParser<T> : IOptionParser where T : IParseResult
    {
        Type IOptionParser.OptionType => typeof(T);
    }

    public record NoOptions();
}
