using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.LastFm.TypeReaders
{
    public enum LastFmPeriod
    {
        SevenDay,
        ThreeMonth,
        SixMonth,
        TwelveMonth,
        Overall
    }

    public class LastFmPeriodStringMapper
    {
        public string MapLastFmPeriodToUrlString(LastFmPeriod lastFmPeriod)
        {
            return lastFmPeriod switch
            {
                LastFmPeriod.SevenDay => "7day",
                LastFmPeriod.ThreeMonth => "3month",
                LastFmPeriod.SixMonth => "6month",
                LastFmPeriod.TwelveMonth => "12month",
                LastFmPeriod.Overall => "overall",
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
                    return Task.FromResult(TypeReaderResult.FromSuccess(
                        LastFmPeriod.SevenDay
                    ));
                case "3m":
                case "3month":
                case "3months":
                    return Task.FromResult(TypeReaderResult.FromSuccess(
                        LastFmPeriod.ThreeMonth
                    ));
                case "6m":
                case "6month":
                case "6months":
                    return Task.FromResult(TypeReaderResult.FromSuccess(
                        LastFmPeriod.SixMonth
                    ));
                case "12m":
                case "12month":
                case "12months":
                case "1y":
                case "1year":
                    return Task.FromResult(TypeReaderResult.FromSuccess(
                        LastFmPeriod.TwelveMonth
                    ));
                case "overall":
                case "all":
                    return Task.FromResult(TypeReaderResult.FromSuccess(
                        LastFmPeriod.Overall
                    ));
                default:
                    return Task.FromResult(TypeReaderResult.FromError(
                        CommandError.ParseFailed,
                        $"Could not parse '{input}' into a valid Last.fm period. Use one of these: {string.Join(',', new[] { "7day", "3month", "6month", "12month", "overall" }.Select(p => $"`{p}`"))}."
                    ));
            }
        }
    }
}
