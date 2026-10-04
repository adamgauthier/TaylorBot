using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class GuildUpdatedHandler(EntityTrackerDomainService entityTrackerDomainService, TaskExceptionLogger taskExceptionLogger, BackgroundTasks backgroundTasks) : IGuildUpdatedHandler
{
    public Task GuildUpdatedAsync(SocketGuild oldGuild, SocketGuild newGuild)
    {
        _ = backgroundTasks.Queue(async () => await taskExceptionLogger.LogOnError(
            entityTrackerDomainService.OnGuildUpdatedAsync(oldGuild, newGuild), nameof(entityTrackerDomainService.OnGuildUpdatedAsync)
        ), nameof(GuildUpdatedHandler));
        return Task.CompletedTask;
    }
}
