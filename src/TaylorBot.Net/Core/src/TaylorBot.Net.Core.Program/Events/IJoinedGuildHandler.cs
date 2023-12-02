using Discord.WebSocket;

namespace TaylorBot.Net.Core.Program.Events;

public interface IJoinedGuildHandler
{
    Task JoinedGuildAsync(SocketGuild guild);
}
