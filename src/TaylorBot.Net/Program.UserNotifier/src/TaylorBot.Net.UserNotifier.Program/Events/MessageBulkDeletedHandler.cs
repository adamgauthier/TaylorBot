using Discord;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class MessageBulkDeletedHandler(TaskExceptionLogger taskExceptionLogger, BackgroundTasks backgroundTasks, MessageLoggerService messageDeletedLoggerService) : IMessageBulkDeletedHandler
{
    public ValueTask MessageBulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, Cacheable<IMessageChannel, ulong> channel)
    {
        _ = backgroundTasks.Queue(async () => await taskExceptionLogger.LogOnError(
            messageDeletedLoggerService.OnMessageBulkDeletedAsync(cachedMessages, await channel.GetOrDownloadAsync()),
            nameof(messageDeletedLoggerService.OnMessageBulkDeletedAsync)
        ), nameof(MessageBulkDeletedHandler));
        return default;
    }
}
