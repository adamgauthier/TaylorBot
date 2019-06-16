using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IGuildUserJoinedHandler
    {
        Task GuildUserJoinedAsync(SocketGuildUser guildUser);
    }
}
