using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.EntityTracker.Domain.TextChannel
{
    public interface ITextChannelRepository
    {
        Task AddTextChannelAsync(ITextChannel textChannel);
        Task AddTextChannelIfNotAddedAsync(ITextChannel textChannel);
    }
}
