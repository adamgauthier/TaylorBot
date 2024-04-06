using Discord;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.MemberLogging.Domain.Options;

namespace TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;

public class GuildMemberLeftEmbedFactory(IOptionsMonitor<MemberLeftLoggingOptions> optionsMonitor)
{
    public Embed CreateMemberLeft(IUser user)
    {
        var options = optionsMonitor.CurrentValue;
        var avatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();

        return new EmbedBuilder()
            .WithAuthor($"{user.Handle()} ({user.Id})", avatarUrl, avatarUrl)
            .WithCurrentTimestamp()
            .WithColor(DiscordColor.FromHexString(options.MemberLeftEmbedColorHex))
            .WithFooter("User left")
            .Build();
    }
}
