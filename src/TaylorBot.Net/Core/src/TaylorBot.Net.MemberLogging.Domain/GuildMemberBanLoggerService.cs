using Discord;
using TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;

namespace TaylorBot.Net.MemberLogging.Domain;

public class GuildMemberBanLoggerService
{
    private readonly MemberLogChannelFinder memberLogChannelFinder;
    private readonly GuildMemberBanEmbedFactory guildMemberBanEmbedFactory;

    public GuildMemberBanLoggerService(
        MemberLogChannelFinder memberLogChannelFinder,
        GuildMemberBanEmbedFactory guildMemberBanEmbedFactory)
    {
        this.memberLogChannelFinder = memberLogChannelFinder;
        this.guildMemberBanEmbedFactory = guildMemberBanEmbedFactory;
    }

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
