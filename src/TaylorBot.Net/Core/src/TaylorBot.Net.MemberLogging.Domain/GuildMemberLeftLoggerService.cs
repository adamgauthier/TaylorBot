using Discord;
using TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;

namespace TaylorBot.Net.MemberLogging.Domain;

public class GuildMemberLeftLoggerService(MemberLogChannelFinder memberLogChannelFinder, GuildMemberLeftEmbedFactory guildMemberLeftEmbedFactory)
{
    public async Task OnGuildMemberLeftAsync(IGuild guild, IUser user)
    {
        var logTextChannel = await memberLogChannelFinder.FindLogChannelAsync(guild);

        if (logTextChannel != null)
            await logTextChannel.SendMessageAsync(embed: guildMemberLeftEmbedFactory.CreateMemberLeft(user));
    }
}
