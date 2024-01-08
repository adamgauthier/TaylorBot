using Discord;
using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class MessageUpdatedHandler(TaskExceptionLogger taskExceptionLogger, MessageLoggerService messageDeletedLoggerService) : IMessageUpdatedHandler
{
    public ValueTask MessageUpdatedAsync(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage, ISocketMessageChannel channel)
    {
        _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
            messageDeletedLoggerService.OnMessageUpdatedAsync(oldMessage, newMessage, channel),
            nameof(messageDeletedLoggerService.OnMessageUpdatedAsync)
        ));
        return default;
    }
}
