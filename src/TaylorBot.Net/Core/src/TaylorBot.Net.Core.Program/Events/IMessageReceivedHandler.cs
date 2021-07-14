using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IMessageReceivedHandler
    {
        Task MessageReceivedAsync(SocketMessage message);
    }
}
