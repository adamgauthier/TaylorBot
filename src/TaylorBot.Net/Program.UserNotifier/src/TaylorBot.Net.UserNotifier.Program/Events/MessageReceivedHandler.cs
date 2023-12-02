using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class MessageReceivedHandler : IMessageReceivedHandler
{
    private readonly MessageLoggerService _messageDeletedLoggerService;

    public MessageReceivedHandler(MessageLoggerService messageDeletedLoggerService)
    {
        _messageDeletedLoggerService = messageDeletedLoggerService;
    }

    public async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Channel is SocketTextChannel textChannel)
        {
            await _messageDeletedLoggerService.OnGuildUserMessageReceivedAsync(textChannel, message);
        }
    }
}
