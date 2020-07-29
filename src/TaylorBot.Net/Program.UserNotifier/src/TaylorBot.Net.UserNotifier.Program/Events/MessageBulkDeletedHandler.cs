using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events
{
    public class MessageBulkDeletedHandler : IMessageBulkDeletedHandler
    {
        private readonly TaskExceptionLogger _taskExceptionLogger;
        private readonly MessageDeletedLoggerService _messageDeletedLoggerService;

        public MessageBulkDeletedHandler(TaskExceptionLogger taskExceptionLogger, MessageDeletedLoggerService messageDeletedLoggerService)
        {
            _taskExceptionLogger = taskExceptionLogger;
            _messageDeletedLoggerService = messageDeletedLoggerService;
        }

        public ValueTask MessageBulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, ISocketMessageChannel channel)
        {
            Task.Run(async () => await _taskExceptionLogger.LogOnError(
                _messageDeletedLoggerService.OnMessageBulkDeletedAsync(cachedMessages, channel),
                nameof(_messageDeletedLoggerService.OnMessageDeletedAsync)
            ));
            return default;
        }
    }
}
