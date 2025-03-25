using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmPeriodStringMapper
{
    public string MapLastFmPeriodToUrlString(LastFmPeriod lastFmPeriod)
    {
        return lastFmPeriod switch
        {
            LastFmPeriod.SevenDay => "7day",
            LastFmPeriod.OneMonth => "1month",
            LastFmPeriod.ThreeMonth => "3month",
            LastFmPeriod.SixMonth => "6month",
            LastFmPeriod.TwelveMonth => "12month",
            LastFmPeriod.Overall => "overall",
            _ => throw new ArgumentOutOfRangeException(nameof(lastFmPeriod)),
        };
    }

    public string MapLastFmPeriodToReadableString(LastFmPeriod lastFmPeriod)
    {
        return lastFmPeriod switch
        {
            LastFmPeriod.SevenDay => "Last 7 days",
            LastFmPeriod.OneMonth => "Last 30 days",
            LastFmPeriod.ThreeMonth => "Last 90 days",
            LastFmPeriod.SixMonth => "Last 180 days",
            LastFmPeriod.TwelveMonth => "Last 365 days",
            LastFmPeriod.Overall => "All time",
            _ => throw new ArgumentOutOfRangeException(nameof(lastFmPeriod)),
        };
    }
}

public class OptionalLastFmPeriodParser : IOptionParser<LastFmPeriod?>
{
    private static readonly string[] Suggestions = ["7day", "1month", "3month", "6month", "12month", "all"];

    public static Result<LastFmPeriod, ParsingFailed> Parse(string input)
    {
        return input.Trim().ToUpperInvariant() switch
        {
            "7D" or "7DAY" or "7DAYS" or "1WEEK" or "WEEK" => Ok(
                LastFmPeriod.SevenDay
            ),
            "1M" or "1MONTH" or "1MONTHS" or "MONTH" or "30DAY" or "30DAYS" => Ok(
                LastFmPeriod.OneMonth
            ),
            "3M" or "3MONTH" or "3MONTHS" or "90DAY" or "90DAYS" => Ok(
                LastFmPeriod.ThreeMonth
            ),
            "6M" or "6MONTH" or "6MONTHS" or "180DAY" or "180DAYS" => Ok(
                LastFmPeriod.SixMonth
            ),
            "12M" or "12MONTH" or "12MONTHS" or "1Y" or "1YEAR" or "365DAY" or "365DAYS" => Ok(
                LastFmPeriod.TwelveMonth
            ),
            "OVERALL" or "ALL" or "ALLTIME" => Ok(
                LastFmPeriod.Overall
            ),
            _ => Error(new ParsingFailed(
                $"Could not parse '{input}' into a valid Last.fm period. Use one of these: {string.Join(',', Suggestions.Select(p => $"`{p}`"))}."
            )),
        };
    }

    public ValueTask<Result<LastFmPeriod?, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        if (!optionValue.HasValue)
        {
            return new((LastFmPeriod?)null);
        }

        var input = optionValue.Value.GetString()!;
        var result = Parse(input);

        return new(
            result.IsSuccess ? (LastFmPeriod?)result.Value : Error(result.Error)
        );
    }
}
