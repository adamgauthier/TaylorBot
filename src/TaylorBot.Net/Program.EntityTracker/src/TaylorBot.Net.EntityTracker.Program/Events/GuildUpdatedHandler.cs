using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.EntityTracker.Program.Events;

public class GuildUpdatedHandler : IGuildUpdatedHandler
{
    private readonly EntityTrackerDomainService entityTrackerDomainService;
    private readonly TaskExceptionLogger taskExceptionLogger;

    public GuildUpdatedHandler(EntityTrackerDomainService entityTrackerDomainService, TaskExceptionLogger taskExceptionLogger)
    {
        this.entityTrackerDomainService = entityTrackerDomainService;
        this.taskExceptionLogger = taskExceptionLogger;
    }

    public Task GuildUpdatedAsync(SocketGuild oldGuild, SocketGuild newGuild)
    {
        Task.Run(async () => await taskExceptionLogger.LogOnError(
            entityTrackerDomainService.OnGuildUpdatedAsync(oldGuild, newGuild), nameof(entityTrackerDomainService.OnGuildUpdatedAsync)
        ));
        return Task.CompletedTask;
    }
}
