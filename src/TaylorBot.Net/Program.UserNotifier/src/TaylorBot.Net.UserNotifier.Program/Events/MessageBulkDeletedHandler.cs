using Discord;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class MessageBulkDeletedHandler(TaskExceptionLogger taskExceptionLogger, MessageLoggerService messageDeletedLoggerService) : IMessageBulkDeletedHandler
{
    public ValueTask MessageBulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, Cacheable<IMessageChannel, ulong> channel)
    {
        Task.Run(async () => await taskExceptionLogger.LogOnError(
            messageDeletedLoggerService.OnMessageBulkDeletedAsync(cachedMessages, await channel.GetOrDownloadAsync()),
            nameof(messageDeletedLoggerService.OnMessageDeletedAsync)
        ));
        return default;
    }
}
