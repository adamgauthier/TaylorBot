using Discord;
using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class ReactionRemovedHandler(TaskExceptionLogger taskExceptionLogger, MessageLoggerService messageDeletedLoggerService) : IReactionRemovedHandler
{
    public ValueTask ReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
            messageDeletedLoggerService.OnReactionRemovedAsync(message, await channel.GetOrDownloadAsync(), reaction),
            nameof(messageDeletedLoggerService.OnReactionRemovedAsync)
        ));
        return default;
    }
}
