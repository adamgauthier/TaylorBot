using Discord;
using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain
{
    public interface ICanUserRedeemResult { }

    public record UserCanRedeem() : ICanUserRedeemResult;

    public record UserCantRedeem(DateTimeOffset CanRedeemAt) : ICanUserRedeemResult;

    public record RedeemResult(long PayoutAmount, long BonusAmount, long TotalTaypointCount, long CurrentDailyStreak, uint DaysForBonus);

    public interface IDailyPayoutRepository
    {
        ValueTask<ICanUserRedeemResult> CanUserRedeemAsync(IUser user);
        ValueTask<RedeemResult?> RedeemDailyPayoutAsync(IUser user);
    }
}

