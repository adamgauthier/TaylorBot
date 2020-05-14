using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IMessageDeletedHandler
    {
        Task UserMessageDeletedAsync(Cacheable<IMessage, ulong> cachedMessage, ISocketMessageChannel channel);
    }
}
