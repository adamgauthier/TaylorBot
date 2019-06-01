using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IGuildUpdatedHandler
    {
        Task GuildUpdatedAsync(SocketGuild oldGuild, SocketGuild newGuild);
    }
}
