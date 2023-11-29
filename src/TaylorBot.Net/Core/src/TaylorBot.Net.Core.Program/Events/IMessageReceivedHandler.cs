using Discord.WebSocket;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IMessageReceivedHandler
    {
        Task MessageReceivedAsync(SocketMessage message);
    }
}
