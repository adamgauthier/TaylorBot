using OperationResult;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands
{
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
        public static Result<LastFmPeriod, ParsingFailed> Parse(string input)
        {
            return input.Trim().ToLowerInvariant() switch
            {
                "7d" or "7day" or "7days" or "1week" or "week" => Ok(
                    LastFmPeriod.SevenDay
                ),
                "1m" or "1month" or "1months" or "month" or "30day" or "30days" => Ok(
                  LastFmPeriod.OneMonth
                ),
                "3m" or "3month" or "3months" or "90day" or "90days" => Ok(
                  LastFmPeriod.ThreeMonth
                ),
                "6m" or "6month" or "6months" or "180day" or "180days" => Ok(
                  LastFmPeriod.SixMonth
                ),
                "12m" or "12month" or "12months" or "1y" or "1year" or "365day" or "365days" => Ok(
                  LastFmPeriod.TwelveMonth
                ),
                "overall" or "all" or "alltime" => Ok(
                  LastFmPeriod.Overall
                ),
                _ => Error(new ParsingFailed(
                  $"Could not parse '{input}' into a valid Last.fm period. Use one of these: {string.Join(',', new[] { "7day", "1month", "3month", "6month", "12month", "all" }.Select(p => $"`{p}`"))}."
                )),
            };
        }

        public ValueTask<Result<LastFmPeriod?, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
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
}
