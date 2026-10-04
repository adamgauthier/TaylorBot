using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class TextChannelCreatedHandler(TaskExceptionLogger taskExceptionLogger, BackgroundTasks backgroundTasks, EntityTrackerDomainService entityTrackerDomainService) : ITextChannelCreatedHandler
{
    public Task TextChannelCreatedAsync(SocketTextChannel textChannel)
    {
        _ = backgroundTasks.Queue(async () => await taskExceptionLogger.LogOnError(
            entityTrackerDomainService.OnTextChannelCreatedAsync(textChannel), nameof(entityTrackerDomainService.OnTextChannelCreatedAsync)
        ), nameof(TextChannelCreatedHandler));
        return Task.CompletedTask;
    }
}
