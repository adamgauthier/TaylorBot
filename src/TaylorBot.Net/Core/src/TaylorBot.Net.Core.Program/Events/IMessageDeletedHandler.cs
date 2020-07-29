using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IMessageDeletedHandler
    {
        ValueTask MessageDeletedAsync(Cacheable<IMessage, ulong> cachedMessage, ISocketMessageChannel channel);
    }
}
