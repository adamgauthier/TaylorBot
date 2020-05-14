using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.EntityTracker.Domain.GuildName
{
    public interface IGuildNameRepository
    {
        ValueTask AddNewGuildNameAsync(IGuild guild);
    }
}
