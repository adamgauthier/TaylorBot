using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class UsernameJoinedGuildHandler(EntityTrackerDomainService entityTrackerDomainService, TaskExceptionLogger taskExceptionLogger, BackgroundTasks backgroundTasks) : IJoinedGuildHandler
{
    public Task JoinedGuildAsync(SocketGuild guild)
    {
        _ = backgroundTasks.Queue(async () => await taskExceptionLogger.LogOnError(
            entityTrackerDomainService.OnGuildJoinedAsync(guild, downloadAllUsers: true), nameof(entityTrackerDomainService.OnGuildJoinedAsync)
        ), nameof(UsernameJoinedGuildHandler));
        return Task.CompletedTask;
    }
}
