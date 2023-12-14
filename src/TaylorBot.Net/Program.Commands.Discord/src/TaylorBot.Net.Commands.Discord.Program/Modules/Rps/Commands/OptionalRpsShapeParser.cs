using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

public enum RpsShape
{
    Rock,
    Paper,
    Scissors,
}

public class OptionalRpsShapeParser : IOptionParser<RpsShape?>
{
    private static readonly string[] Suggestions = ["rock", "paper", "scissors"];

    public static Result<RpsShape, ParsingFailed> Parse(string input)
    {
        return input.Trim().ToLowerInvariant() switch
        {
            "rock" or "r" => Ok(RpsShape.Rock),
            "paper" or "p" => Ok(RpsShape.Paper),
            "scissors" or "s" => Ok(RpsShape.Scissors),
            _ => Error(new ParsingFailed(
                $"Could not parse '{input}' into a valid rps shape. Use one of these: {string.Join(',', Suggestions.Select(p => $"`{p}`"))}."
            )),
        };
    }

    public ValueTask<Result<RpsShape?, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        if (!optionValue.HasValue)
        {
            return new((RpsShape?)null);
        }

        var input = optionValue.Value.GetString()!;
        var result = Parse(input);

        return new(
            result.IsSuccess ? (RpsShape?)result.Value : Error(result.Error)
        );
    }
}
