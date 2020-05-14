using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IGuildUserBannedHandler
    {
        Task GuildUserBannedAsync(SocketUser user, SocketGuild guild);
    }
}
