using Discord;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class MessageDeletedHandler(TaskExceptionLogger taskExceptionLogger, MessageLoggerService messageDeletedLoggerService) : IMessageDeletedHandler
{
    public ValueTask MessageDeletedAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel)
    {
        Task.Run(async () => await taskExceptionLogger.LogOnError(
            messageDeletedLoggerService.OnMessageDeletedAsync(cachedMessage, await channel.GetOrDownloadAsync()),
            nameof(messageDeletedLoggerService.OnMessageDeletedAsync)
        ));
        return default;
    }
}
