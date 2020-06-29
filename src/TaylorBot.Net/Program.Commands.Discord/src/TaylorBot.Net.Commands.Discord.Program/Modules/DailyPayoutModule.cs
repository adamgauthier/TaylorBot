using Discord;
using Discord.Commands;
using Humanizer;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.DailyPayout.Domain;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [Name("Daily Payout 👔")]
    [Group("daily")]
    [Alias("dailypayout")]
    public class DailyPayoutModule : TaylorBotModule
    {
        private readonly IDailyPayoutRepository _dailyPayoutRepository;

        public DailyPayoutModule(IDailyPayoutRepository dailyPayoutRepository)
        {
            _dailyPayoutRepository = dailyPayoutRepository;
        }

        [Command]
        [Summary("Awards you with your daily amount of taypoints.")]
        public async Task<RuntimeResult> DailyAsync()
        {
            var embed = new EmbedBuilder()
                .WithUserAsAuthor(Context.User);

            var canRedeem = await _dailyPayoutRepository.CanUserRedeemAsync(Context.User);

            if (canRedeem is UserCantRedeem userCantRedeem)
            {
                return new TaylorBotEmbedResult(embed
                    .WithColor(TaylorBotColors.ErrorColor)
                    .WithDescription(string.Join('\n', new[] {
                        "You've already redeemed your daily payout today.",
                        $"You can redeem again **{userCantRedeem.CanRedeemAt.Humanize(culture: TaylorBotCulture.Culture)}**."
                    }))
                .Build());
            }

            var redeemResult = await _dailyPayoutRepository.RedeemDailyPayoutAsync(Context.User);

            if (redeemResult == null)
            {
                return new TaylorBotEmbedResult(embed
                    .WithColor(TaylorBotColors.ErrorColor)
                    .WithDescription("You've already redeemed your daily payout today.")
                .Build());
            }

            var nextStreakForBonus = (redeemResult.CurrentDailyStreak - redeemResult.CurrentDailyStreak % redeemResult.DaysForBonus) + redeemResult.DaysForBonus;
            var format = TaylorBotFormats.BoldReadable;

            return new TaylorBotEmbedResult(embed
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(string.Join('\n', new[] {
                    $"You redeemed {"taypoint".ToQuantity(redeemResult.PayoutAmount, format)} + {"bonus point".ToQuantity(redeemResult.BonusAmount, format)}." +
                        $" You now have {redeemResult.TotalTaypointCount.ToString(format)}. 💰",
                    $"Bonus streak: {redeemResult.CurrentDailyStreak.ToString(format)}/{nextStreakForBonus.ToString(format)}." +
                        " Don't miss a day and get a bonus! See you tomorrow! 😄"
                }))
            .Build());
        }
    }
}

