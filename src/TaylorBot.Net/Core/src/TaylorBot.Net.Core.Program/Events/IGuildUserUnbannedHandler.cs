using Discord.WebSocket;

namespace TaylorBot.Net.Core.Program.Events;

public interface IGuildUserUnbannedHandler
{
    Task GuildUserUnbannedAsync(SocketUser user, SocketGuild guild);
}
