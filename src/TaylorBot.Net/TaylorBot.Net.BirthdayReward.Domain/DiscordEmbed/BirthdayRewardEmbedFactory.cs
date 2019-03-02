using Discord;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Strings;

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
                    $"I gave you {"birthday point".DisplayCount(rewardAmount, "**")} as a gift! 🎁",
                    $"You now have {rewardedUser.PointsAfterReward}! 💰"
                }))
                .Build();
        }
    }
}
