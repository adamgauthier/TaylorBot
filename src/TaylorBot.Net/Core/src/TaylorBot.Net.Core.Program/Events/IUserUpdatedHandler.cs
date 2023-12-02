using Discord.WebSocket;

namespace TaylorBot.Net.Core.Program.Events;

public interface IUserUpdatedHandler
{
    Task UserUpdatedAsync(SocketUser oldUser, SocketUser newUser);
}
