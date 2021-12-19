using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IGuildUserLeftHandler
    {
        Task GuildUserLeftAsync(SocketGuild guild, SocketUser user);
    }
}
