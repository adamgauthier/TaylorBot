using Discord;
using Humanizer;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;

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
                    $"I gave you {"birthday point".ToQuantity(rewardAmount, TaylorBotFormats.BoldReadable)} as a gift! 🎁",
                    $"You now have {rewardedUser.PointsAfterReward.ToString(TaylorBotFormats.Readable)}! 💰"
                }))
                .Build();
        }
    }
}
