using Discord;
using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events
{
    public class MessageUpdatedHandler : IMessageUpdatedHandler
    {
        private readonly TaskExceptionLogger _taskExceptionLogger;
        private readonly MessageLoggerService _messageDeletedLoggerService;

        public MessageUpdatedHandler(TaskExceptionLogger taskExceptionLogger, MessageLoggerService messageDeletedLoggerService)
        {
            _taskExceptionLogger = taskExceptionLogger;
            _messageDeletedLoggerService = messageDeletedLoggerService;
        }

        public ValueTask MessageUpdatedAsync(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage, ISocketMessageChannel channel)
        {
            _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(
                _messageDeletedLoggerService.OnMessageUpdatedAsync(oldMessage, newMessage, channel),
                nameof(_messageDeletedLoggerService.OnMessageUpdatedAsync)
            ));
            return default;
        }
    }
}
