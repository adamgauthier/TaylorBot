using Discord;
using Humanizer;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.PatreonSync.Domain.DiscordEmbed;

public sealed class PatreonUpdateEmbedFactory
{
    public static Embed? Create(IUpdatePlusUserResult result)
    {
        switch (result)
        {
            case ActiveUserAdded added:
                return new EmbedBuilder()
                    .WithColor(TaylorBotColors.DiamondBlueColor)
                    .WithDescription(
                        $"""
                        🥳 **Welcome to TaylorBot Plus** 🥳
                        I just detected that you are now supporting me on Patreon, thank you so much! 🥺
                        This gives you access to exclusive features for you and your favorite servers! ⚡
                        Patreon only charges you at the start of every month, and when that happens, you'll also receive taypoints! 🎁
                        To learn more, take a look at your membership status by typing </plus show:1246970937321066608> 👀
                        """)
                .Build();

            case UserRewarded rewarded:
                return new EmbedBuilder()
                    .WithColor(TaylorBotColors.DiamondBlueColor)
                    .WithDescription(
                        $"""
                        🥺 **Thank you for supporting TaylorBot** 🥺
                        As part of your TaylorBot Plus membership, I gave you {"point".ToQuantity(rewarded.Reward, TaylorBotFormats.BoldReadable)} as a gift! 🎁
                        You now have {rewarded.NewTaypointCount.ToString(TaylorBotFormats.Readable)}! 💰
                        """)
                .Build();

            case GuildsDisabledForInactivity inactivity:
                return new EmbedBuilder()
                    .WithColor(TaylorBotColors.DiamondBlueColor)
                    .WithDescription(
                        $"""
                        📝 **Changes in your TaylorBot Plus membership** 📝
                        It seems like you cancelled or declined your Patreon pledge for this month. 😕
                        This is okay, thank you for even considering supporting in the first place! 😊
                        This message is to inform you that your TaylorBot Plus servers are losing access to exclusive features:

                        {string.Join('\n', inactivity.DisabledGuilds.Select(name => $"- {name}"))}

                        If you think this is a mistake, message the bot owner on Discord or Patreon! ✉
                        """.Truncate(EmbedBuilder.MaxDescriptionLength))
                .Build();

            case GuildsDisabledForLoweredPledge loweredPledge:
                return new EmbedBuilder()
                    .WithColor(TaylorBotColors.DiamondBlueColor)
                    .WithDescription(
                    $"""
                    📝 **Changes in your TaylorBot Plus membership** 📝
                    It seems like your Patreon pledge was lowered. 😦
                    This message is to inform you that your new pledge can't cover all your TaylorBot Plus servers (**{loweredPledge.DisabledGuilds.Count}/{loweredPledge.MaxPlusGuilds}**).
                    These servers have lost access to exclusive features, **you need to re-enable them manually** (up to {loweredPledge.MaxPlusGuilds}):

                    {string.Join('\n', loweredPledge.DisabledGuilds.Select(name => $"- {name}"))}

                    If you think this is a mistake, message the bot owner on Discord or Patreon! ✉
                    """.Truncate(EmbedBuilder.MaxDescriptionLength))
                .Build();

            default: return null;
        }
    }
}
