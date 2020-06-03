using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.MemberLogging.Domain.TextChannel
{
    public interface IMemberLoggingChannelRepository
    {
        ValueTask<IReadOnlyCollection<LogChannel>> GetLogChannelsForGuildAsync(IGuild guild);
    }
}
