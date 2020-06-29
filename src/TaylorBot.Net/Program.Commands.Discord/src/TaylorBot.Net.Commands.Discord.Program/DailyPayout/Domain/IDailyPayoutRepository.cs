using Discord;
using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.DailyPayout.Domain
{
    public interface ICanUserRedeemResult { }
    public class UserCanRedeem : ICanUserRedeemResult { }
    public class UserCantRedeem : ICanUserRedeemResult
    {
        public DateTimeOffset CanRedeemAt { get; }

        public UserCantRedeem(DateTimeOffset canRedeemAt) => CanRedeemAt = canRedeemAt;
    }

    public class RedeemResult
    {
        public long PayoutAmount { get; }
        public long BonusAmount { get; }
        public long TotalTaypointCount { get; }
        public long CurrentDailyStreak { get; }
        public uint DaysForBonus { get; }

        public RedeemResult(long payoutAmount, long bonusAmount, long totalTaypointCount, long currentDailyStreak, uint daysForBonus)
        {
            PayoutAmount = payoutAmount;
            BonusAmount = bonusAmount;
            TotalTaypointCount = totalTaypointCount;
            CurrentDailyStreak = currentDailyStreak;
            DaysForBonus = daysForBonus;
        }
    }

    public interface IDailyPayoutRepository
    {
        ValueTask<ICanUserRedeemResult> CanUserRedeemAsync(IUser user);
        ValueTask<RedeemResult> RedeemDailyPayoutAsync(IUser user);
    }
}

