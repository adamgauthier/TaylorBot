using Discord;
using OperationResult;
using TaylorBot.Net.Core.Snowflake;

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
    ValueTask<ICanUserRedeemResult> CanUserRedeemAsync(IUser user);
    ValueTask<RedeemResult?> RedeemDailyPayoutAsync(IUser user, uint payoutAmount);
    ValueTask<(long CurrentStreak, long MaxStreak)?> GetStreakInfoAsync(IUser user);
    ValueTask<Result<RebuyResult, RebuyFailed>> RebuyMaxStreakAsync(IUser user, int pricePerDay);
    ValueTask<IList<DailyLeaderboardEntry>> GetLeaderboardAsync(IGuild guild);
}

