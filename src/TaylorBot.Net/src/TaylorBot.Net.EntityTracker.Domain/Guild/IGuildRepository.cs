using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.EntityTracker.Domain.Guild
{
    public interface IGuildRepository
    {
        ValueTask<GuildAddedResult> AddGuildIfNotAddedAsync(IGuild guild);
    }
}
