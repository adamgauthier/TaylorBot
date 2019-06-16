using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IJoinedGuildHandler
    {
        Task JoinedGuildAsync(SocketGuild guild);
    }
}
