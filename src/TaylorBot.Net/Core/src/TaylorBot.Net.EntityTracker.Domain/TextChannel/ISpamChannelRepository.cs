using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.EntityTracker.Domain.TextChannel
{
    public interface ISpamChannelRepository
    {
        ValueTask<bool> InsertOrGetIsSpamChannelAsync(ITextChannel channel);
    }
}
