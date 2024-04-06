using OperationResult;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;

public interface ICanUserRedeemResult { }

public record UserCanRedeem() : ICanUserRedeemResult;

public record UserCantRedeem(DateTimeOffset CanRedeemAt) : ICanUserRedeemResult;

public record RedeemResult(long BonusAmount, long TotalTaypointCount, long CurrentDailyStreak, uint DaysForBonus);

public record RebuyResult(long TotalTaypointCount, long CurrentDailyStreak);

public record RebuyFailed(long TotalTaypointCount);

public record DailyLeaderboardEntry(SnowflakeId UserId, string Username, long CurrentDailyStreak, long Rank);

public interface IDailyPayoutRepository
{
    ValueTask<ICanUserRedeemResult> CanUserRedeemAsync(DiscordUser user);
    ValueTask<RedeemResult?> RedeemDailyPayoutAsync(DiscordUser user, uint payoutAmount);
    ValueTask<(long CurrentStreak, long MaxStreak)?> GetStreakInfoAsync(DiscordUser user);
    ValueTask<Result<RebuyResult, RebuyFailed>> RebuyMaxStreakAsync(DiscordUser user, int pricePerDay);
    ValueTask<IList<DailyLeaderboardEntry>> GetLeaderboardAsync(CommandGuild guild);
}

