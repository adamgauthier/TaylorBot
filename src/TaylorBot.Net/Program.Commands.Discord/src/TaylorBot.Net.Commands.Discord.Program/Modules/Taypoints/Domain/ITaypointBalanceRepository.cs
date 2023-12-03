using Discord;
using Humanizer;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Snowflake;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;

public record TaypointBalance(long TaypointCount, int? ServerRank);

public record TaypointLeaderboardEntry(SnowflakeId UserId, string Username, long TaypointCount, long Rank);

public interface ITaypointBalanceRepository
{
    ValueTask<TaypointBalance> GetBalanceAsync(IUser user);
    ValueTask<TaypointBalance> GetBalanceWithRankAsync(IGuildUser user);
    ValueTask<IList<TaypointLeaderboardEntry>> GetLeaderboardAsync(IGuild guild);
}


public interface ITaypointAmount { }

public record AbsoluteTaypointAmount(long Amount, long Balance) : ITaypointAmount;

public record RelativeTaypointAmount(byte Proportion) : ITaypointAmount;

public class TaypointAmountParser(StringParser stringParser, ITaypointBalanceRepository taypointBalanceRepository) : IOptionParser<ITaypointAmount>
{
    public async ValueTask<Result<ITaypointAmount, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsed = await stringParser.ParseAsync(context, optionValue, resolved);

        if (parsed)
        {
            var text = parsed.Value.Value;

            switch (text.ToLowerInvariant())
            {
                case "all":
                    return new RelativeTaypointAmount(1);

                case "half":
                    return new RelativeTaypointAmount(2);

                case "third":
                    return new RelativeTaypointAmount(3);

                case "fourth":
                    return new RelativeTaypointAmount(4);

                default:
                    if (long.TryParse(text, out var amount))
                    {
                        if (amount > 0)
                        {
                            var balance = await taypointBalanceRepository.GetBalanceAsync(context.User);

                            if (amount > balance.TaypointCount)
                            {
                                return Error(new ParsingFailed($"You can't spend {"taypoint".ToQuantity(amount, TaylorBotFormats.BoldReadable)}, you only have {balance.TaypointCount.ToString(TaylorBotFormats.BoldReadable)}. 😕"));
                            }

                            return new AbsoluteTaypointAmount(amount, balance.TaypointCount);
                        }
                        else
                        {
                            return Error(new ParsingFailed("Amount must be higher than 0."));
                        }
                    }
                    else
                    {
                        return Error(new ParsingFailed("Must be a valid number or fraction ('all', 'half' or 'third')."));
                    }
            }
        }
        else
        {
            return Error(parsed.Error);
        }
    }
}
