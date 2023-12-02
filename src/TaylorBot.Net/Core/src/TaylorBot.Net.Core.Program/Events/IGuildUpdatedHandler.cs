using Discord.WebSocket;

namespace TaylorBot.Net.Core.Program.Events;

public interface IGuildUpdatedHandler
{
    Task GuildUpdatedAsync(SocketGuild oldGuild, SocketGuild newGuild);
}
