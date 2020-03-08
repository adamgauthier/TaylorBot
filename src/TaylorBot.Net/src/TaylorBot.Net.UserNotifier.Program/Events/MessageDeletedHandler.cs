using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events
{
    public class MessageDeletedHandler : IMessageDeletedHandler
    {
        private readonly TaskExceptionLogger _taskExceptionLogger;
        private readonly MessageDeletedLoggerService _messageDeletedLoggerService;

        public MessageDeletedHandler(TaskExceptionLogger taskExceptionLogger, MessageDeletedLoggerService messageDeletedLoggerService)
        {
            _taskExceptionLogger = taskExceptionLogger;
            _messageDeletedLoggerService = messageDeletedLoggerService;
        }

        public Task UserMessageDeletedAsync(Cacheable<IMessage, ulong> cachedMessage, ISocketMessageChannel channel)
        {
            Task.Run(async () => await _taskExceptionLogger.LogOnError(
                _messageDeletedLoggerService.OnMessageDeletedAsync(cachedMessage, channel), nameof(_messageDeletedLoggerService.OnMessageDeletedAsync)
            ));
            return Task.CompletedTask;
        }
    }
}
