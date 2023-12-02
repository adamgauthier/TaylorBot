using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.EntityTracker.Program.Events;

public class TextChannelCreatedHandler : ITextChannelCreatedHandler
{
    private readonly TaskExceptionLogger taskExceptionLogger;
    private readonly EntityTrackerDomainService entityTrackerDomainService;

    public TextChannelCreatedHandler(TaskExceptionLogger taskExceptionLogger, EntityTrackerDomainService entityTrackerDomainService)
    {
        this.taskExceptionLogger = taskExceptionLogger;
        this.entityTrackerDomainService = entityTrackerDomainService;
    }

    public Task TextChannelCreatedAsync(SocketTextChannel textChannel)
    {
        Task.Run(async () => await taskExceptionLogger.LogOnError(
            entityTrackerDomainService.OnTextChannelCreatedAsync(textChannel), nameof(entityTrackerDomainService.OnTextChannelCreatedAsync)
        ));
        return Task.CompletedTask;
    }
}
