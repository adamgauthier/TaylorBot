using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class MessageReceivedHandler(MessageLoggerService messageDeletedLoggerService) : IMessageReceivedHandler
{
    public async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Channel is SocketTextChannel textChannel)
        {
            await messageDeletedLoggerService.OnGuildUserMessageReceivedAsync(textChannel, message);
        }
    }
}
