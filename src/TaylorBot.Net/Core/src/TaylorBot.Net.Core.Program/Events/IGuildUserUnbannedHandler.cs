using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IGuildUserUnbannedHandler
    {
        Task GuildUserUnbannedAsync(SocketUser user, SocketGuild guild);
    }
}
