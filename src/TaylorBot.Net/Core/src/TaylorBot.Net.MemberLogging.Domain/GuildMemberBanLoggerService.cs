using Discord;
using TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;

namespace TaylorBot.Net.MemberLogging.Domain;

public class GuildMemberBanLoggerService(
    MemberLogChannelFinder memberLogChannelFinder,
    GuildMemberBanEmbedFactory guildMemberBanEmbedFactory)
{
    public async Task OnGuildMemberBannedAsync(IUser user, IGuild guild)
    {
        var logTextChannel = await memberLogChannelFinder.FindLogChannelAsync(guild);

        if (logTextChannel != null)
            await logTextChannel.SendMessageAsync(embed: guildMemberBanEmbedFactory.CreateMemberBanned(user));
    }

    public async Task OnGuildMemberUnbannedAsync(IUser user, IGuild guild)
    {
        var logTextChannel = await memberLogChannelFinder.FindLogChannelAsync(guild);

        if (logTextChannel != null)
            await logTextChannel.SendMessageAsync(embed: guildMemberBanEmbedFactory.CreateMemberUnbanned(user));
    }
}
