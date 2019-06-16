using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.EntityTracker.Domain.Guild
{
    public interface IGuildRepository
    {
        Task<GuildAddedResult> AddGuildIfNotAddedAsync(IGuild guild);
    }
}
