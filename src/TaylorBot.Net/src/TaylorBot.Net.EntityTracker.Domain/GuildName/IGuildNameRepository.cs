using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.EntityTracker.Domain.GuildName
{
    public interface IGuildNameRepository
    {
        Task AddNewGuildNameAsync(IGuild guild);
        Task<string> GetLatestGuildNameAsync(IGuild guild);
    }
}
