using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.EntityTracker.Program.Events;

public class UsernameJoinedGuildHandler(EntityTrackerDomainService entityTrackerDomainService, TaskExceptionLogger taskExceptionLogger) : IJoinedGuildHandler
{
    public Task JoinedGuildAsync(SocketGuild guild)
    {
        Task.Run(async () => await taskExceptionLogger.LogOnError(
            entityTrackerDomainService.OnGuildJoinedAsync(guild, downloadAllUsers: true), nameof(entityTrackerDomainService.OnGuildJoinedAsync)
        ));
        return Task.CompletedTask;
    }
}
