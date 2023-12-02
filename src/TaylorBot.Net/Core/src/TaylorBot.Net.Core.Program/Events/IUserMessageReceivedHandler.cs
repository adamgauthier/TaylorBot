using Discord.WebSocket;

namespace TaylorBot.Net.Core.Program.Events;

public interface IUserMessageReceivedHandler
{
    Task UserMessageReceivedAsync(SocketUserMessage userMessage);
}
