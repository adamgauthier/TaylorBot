using Discord;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class MessageBulkDeletedHandler : IMessageBulkDeletedHandler
{
    private readonly TaskExceptionLogger _taskExceptionLogger;
    private readonly MessageLoggerService _messageDeletedLoggerService;

    public MessageBulkDeletedHandler(TaskExceptionLogger taskExceptionLogger, MessageLoggerService messageDeletedLoggerService)
    {
        _taskExceptionLogger = taskExceptionLogger;
        _messageDeletedLoggerService = messageDeletedLoggerService;
    }

    public ValueTask MessageBulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, Cacheable<IMessageChannel, ulong> channel)
    {
        Task.Run(async () => await _taskExceptionLogger.LogOnError(
            _messageDeletedLoggerService.OnMessageBulkDeletedAsync(cachedMessages, await channel.GetOrDownloadAsync()),
            nameof(_messageDeletedLoggerService.OnMessageDeletedAsync)
        ));
        return default;
    }
}
