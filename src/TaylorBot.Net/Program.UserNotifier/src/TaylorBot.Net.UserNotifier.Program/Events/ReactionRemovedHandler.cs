using Discord;
using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class ReactionRemovedHandler : IReactionRemovedHandler
{
    private readonly TaskExceptionLogger _taskExceptionLogger;
    private readonly MessageLoggerService _messageDeletedLoggerService;

    public ReactionRemovedHandler(TaskExceptionLogger taskExceptionLogger, MessageLoggerService messageDeletedLoggerService)
    {
        _taskExceptionLogger = taskExceptionLogger;
        _messageDeletedLoggerService = messageDeletedLoggerService;
    }

    public ValueTask ReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(
            _messageDeletedLoggerService.OnReactionRemovedAsync(message, await channel.GetOrDownloadAsync(), reaction),
            nameof(_messageDeletedLoggerService.OnReactionRemovedAsync)
        ));
        return default;
    }
}
