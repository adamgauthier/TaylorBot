using Discord;
using Humanizer;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.BirthdayReward.Domain.DiscordEmbed
{
    public class BirthdayRewardEmbedFactory
    {
        public Embed Create(long rewardAmount, RewardedUser rewardedUser)
        {
            return new EmbedBuilder()
                .WithColor(TaylorBotColors.GoldColor)
                .WithDescription(string.Join("\n", new[] {
                    "🎈🎂🎊 **Happy Birthday** 🎊🎂🎈",
                    $"I gave you {"birthday point".ToQuantity(rewardAmount, "**#,0**")} as a gift! 🎁",
                    $"You now have {rewardedUser.PointsAfterReward}! 💰"
                }))
                .Build();
        }
    }
}
