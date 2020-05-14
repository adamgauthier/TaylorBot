using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.MemberLogging.Domain.TextChannel
{
    public interface IMemberLoggingChannelRepository
    {
        Task<IEnumerable<LogChannel>> GetLogChannelsForGuildAsync(IGuild guild);
    }
}
