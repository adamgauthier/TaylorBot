using Discord;
using Discord.WebSocket;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IMessageUpdatedHandler
    {
        ValueTask MessageUpdatedAsync(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage, ISocketMessageChannel channel);
    }
}
