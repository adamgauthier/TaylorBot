using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Risk.Commands;

public enum RiskLevel
{
    Low,
    Moderate,
    High,
}

public class OptionalRiskLevelParser : IOptionParser<RiskLevel?>
{
    private static Result<RiskLevel, ParsingFailed> Parse(string input)
    {
        return input.Trim().ToUpperInvariant() switch
        {
            "LOW" => Ok(RiskLevel.Low),
            "MODERATE" => Ok(RiskLevel.Moderate),
            "HIGH" => Ok(RiskLevel.High),
            _ => Error(new ParsingFailed($"Could not parse '{input}' into a valid risk level.")),
        };
    }

    public ValueTask<Result<RiskLevel?, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        if (!optionValue.HasValue)
        {
            return new((RiskLevel?)null);
        }

        var input = optionValue.Value.GetString()!;
        var result = Parse(input);

        return new(
            result.IsSuccess ? (RiskLevel?)result.Value : Error(result.Error)
        );
    }
}
