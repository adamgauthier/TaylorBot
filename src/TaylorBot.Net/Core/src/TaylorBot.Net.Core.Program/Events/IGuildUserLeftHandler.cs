using Discord.WebSocket;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IGuildUserLeftHandler
    {
        Task GuildUserLeftAsync(SocketGuild guild, SocketUser user);
    }
}
