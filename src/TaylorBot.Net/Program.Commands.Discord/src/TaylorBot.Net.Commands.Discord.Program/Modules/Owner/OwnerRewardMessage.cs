using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Owner;

internal static class OwnerRewardMessage
{
    public static string FormatHeader(DiscordUser author, int amount, IReadOnlyCollection<RewardedUserResult> recipients)
    {
        if (recipients.Count == 0)
        {
            return "### Taypoint Reward\nNo users were rewarded.";
        }

        var destination = recipients.Count == 1
            ? MentionUtils.MentionUser(recipients.Single().UserId.Id)
            : $"__{"user".ToQuantity(recipients.Count)}__";
        var each = recipients.Count > 1 ? " each" : "";

        return $"""
            ### Taypoint Reward
            {author.Mention} 🎁 **{"taypoint".ToQuantity(amount, TaylorBotFormats.Readable)}**{each} ➡️ {destination}
            """;
    }

    public static string FormatBalance(RewardedUserResult user, int amount) =>
        $"{MentionUtils.MentionUser(user.UserId.Id)}: {(user.NewTaypointCount - amount).ToString(TaylorBotFormats.BoldReadable)} ➡️ {"taypoint".ToQuantity(user.NewTaypointCount, TaylorBotFormats.BoldReadable)} 📈";
}
