using Discord;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.MemberLogging.Domain.Options;

namespace TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;

public class GuildMemberBanEmbedFactory(IOptionsMonitor<MemberBanLoggingOptions> optionsMonitor)
{
    public Embed CreateMemberBanned(IUser user)
    {
        var options = optionsMonitor.CurrentValue;
        var avatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();

        return new EmbedBuilder()
            .WithAuthor($"{user.Username}{user.DiscrimSuffix()} ({user.Id})", avatarUrl, avatarUrl)
            .WithCurrentTimestamp()
            .WithColor(DiscordColor.FromHexString(options.MemberBannedEmbedColorHex))
            .WithFooter("User banned")
            .Build();
    }

    public Embed CreateMemberUnbanned(IUser user)
    {
        var options = optionsMonitor.CurrentValue;
        var avatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();

        return new EmbedBuilder()
            .WithAuthor($"{user.Username}{user.DiscrimSuffix()} ({user.Id})", avatarUrl, avatarUrl)
            .WithCurrentTimestamp()
            .WithColor(DiscordColor.FromHexString(options.MemberUnbannedEmbedColorHex))
            .WithFooter("User unbanned")
            .Build();
    }
}
