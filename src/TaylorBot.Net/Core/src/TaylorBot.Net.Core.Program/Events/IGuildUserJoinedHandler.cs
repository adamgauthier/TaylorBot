using Discord.WebSocket;

namespace TaylorBot.Net.Core.Program.Events;

public interface IGuildUserJoinedHandler
{
    Task GuildUserJoinedAsync(SocketGuildUser guildUser);
}
