using Humanizer;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;

public record TaypointLeaderboardEntry(string user_id, string username, long last_known_taypoint_count, long rank, long taypoint_count);

public record TaypointCountUpdate(SnowflakeId UserId, long TaypointCount);

public interface ITaypointBalanceRepository
{
    ValueTask<long> GetBalanceAsync(DiscordUser user);
    Task<int> UpdateLastKnownPointCountAsync(DiscordMember member, long updatedCount);
    ValueTask<IList<TaypointLeaderboardEntry>> GetLeaderboardAsync(CommandGuild guild);
    Task UpdateLastKnownPointCountsAsync(CommandGuild guild, IReadOnlyList<TaypointCountUpdate> updates);
    Task<int> UpdateLastKnownPointCountsForRecentlyActiveMembersAsync(CommandGuild guild);
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
            return await ParseStringAsync(context, parsed.Value.Value);
        }
        else
        {
            return Error(parsed.Error);
        }
    }

    public async ValueTask<Result<ITaypointAmount, ParsingFailed>> ParseStringAsync(RunContext context, string text)
    {
        switch (text.ToUpperInvariant())
        {
            case "ALL":
                return new RelativeTaypointAmount(1);

            case "HALF":
                return new RelativeTaypointAmount(2);

            case "THIRD":
                return new RelativeTaypointAmount(3);

            case "FOURTH":
                return new RelativeTaypointAmount(4);

            // TODO: Find a long-term solution for percentages
            case "1%":
                return new RelativeTaypointAmount(100);

            case "2%":
                return new RelativeTaypointAmount(50);

            case "4%":
                return new RelativeTaypointAmount(25);

            case "5%":
                return new RelativeTaypointAmount(20);

            case "10%":
                return new RelativeTaypointAmount(10);

            case "20%":
                return new RelativeTaypointAmount(5);

            case "25%":
                return new RelativeTaypointAmount(4);

            case "33%":
                return new RelativeTaypointAmount(3);

            case "50%":
                return new RelativeTaypointAmount(2);

            case "100%":
                return new RelativeTaypointAmount(1);

            default:
                if (long.TryParse(text, out var amount))
                {
                    if (amount > 0)
                    {
                        var balance = await taypointBalanceRepository.GetBalanceAsync(context.User);

                        if (amount > balance)
                        {
                            return Error(new ParsingFailed($"You can't spend {"taypoint".ToQuantity(amount, TaylorBotFormats.BoldReadable)}, you only have {balance.ToString(TaylorBotFormats.BoldReadable)}. 😕"));
                        }

                        return new AbsoluteTaypointAmount(amount, balance);
                    }
                    else
                    {
                        return Error(new ParsingFailed("Must be higher than 0."));
                    }
                }
                else
                {
                    return Error(new ParsingFailed("Must be a valid number or fraction ('all', 'half' or 'third')."));
                }
        }
    }
}
