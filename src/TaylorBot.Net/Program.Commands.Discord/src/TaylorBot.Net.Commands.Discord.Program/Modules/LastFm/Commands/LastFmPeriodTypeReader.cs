using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;

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

    public class LastFmPeriodTypeReader : TypeReader, ITaylorBotTypeReader
    {
        public Type ArgumentType => typeof(LastFmPeriod);

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            switch (input.Trim().ToLowerInvariant())
            {
                case "7d":
                case "7day":
                case "7days":
                case "1week":
                case "week":
                    return Task.FromResult(TypeReaderResult.FromSuccess(
                        LastFmPeriod.SevenDay
                    ));
                case "1m":
                case "1month":
                case "1months":
                case "30day":
                case "30days":
                    return Task.FromResult(TypeReaderResult.FromSuccess(
                        LastFmPeriod.OneMonth
                    ));
                case "3m":
                case "3month":
                case "3months":
                case "90day":
                case "90days":
                    return Task.FromResult(TypeReaderResult.FromSuccess(
                        LastFmPeriod.ThreeMonth
                    ));
                case "6m":
                case "6month":
                case "6months":
                case "180day":
                case "180days":
                    return Task.FromResult(TypeReaderResult.FromSuccess(
                        LastFmPeriod.SixMonth
                    ));
                case "12m":
                case "12month":
                case "12months":
                case "1y":
                case "1year":
                case "365day":
                case "365days":
                    return Task.FromResult(TypeReaderResult.FromSuccess(
                        LastFmPeriod.TwelveMonth
                    ));
                case "overall":
                case "all":
                case "alltime":
                    return Task.FromResult(TypeReaderResult.FromSuccess(
                        LastFmPeriod.Overall
                    ));
                default:
                    return Task.FromResult(TypeReaderResult.FromError(
                        CommandError.ParseFailed,
                        $"Could not parse '{input}' into a valid Last.fm period. Use one of these: {string.Join(',', new[] { "7day", "1month", "3month", "6month", "12month", "overall" }.Select(p => $"`{p}`"))}."
                    ));
            }
        }
    }
}
