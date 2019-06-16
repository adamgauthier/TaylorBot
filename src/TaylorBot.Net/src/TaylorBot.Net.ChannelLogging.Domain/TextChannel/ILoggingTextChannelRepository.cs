using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.EntityTracker.Domain.TextChannel
{
    public interface ILoggingTextChannelRepository
    {
        Task<IEnumerable<LogChannel>> GetLogChannelsForGuildAsync(IGuild guild);
    }
}
