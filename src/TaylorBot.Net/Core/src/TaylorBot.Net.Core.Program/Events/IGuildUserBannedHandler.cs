using Discord.WebSocket;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IGuildUserBannedHandler
    {
        Task GuildUserBannedAsync(SocketUser user, SocketGuild guild);
    }
}
