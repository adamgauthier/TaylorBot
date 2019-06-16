using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IUserMessageReceivedHandler
    {
        Task UserMessageReceivedAsync(SocketUserMessage userMessage);
    }
}
