using Discord;
using TaylorBot.Net.MemberLogging.Domain.TextChannel;

namespace TaylorBot.Net.MemberLogging.Domain;

public class MemberLogChannelFinder(IMemberLoggingChannelRepository memberLoggingChannelRepository)
{
    public async ValueTask<ITextChannel?> FindLogChannelAsync(IGuild guild)
    {
        var logChannel = await memberLoggingChannelRepository.GetLogChannelForGuildAsync(guild);

        return logChannel != null ?
            (await guild.GetTextChannelsAsync()).FirstOrDefault(c => logChannel.ChannelId.Id == c.Id) :
            null;
    }
}
