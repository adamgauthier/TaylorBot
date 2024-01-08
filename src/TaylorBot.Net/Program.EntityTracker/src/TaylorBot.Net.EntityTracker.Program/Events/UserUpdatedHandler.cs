using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.EntityTracker.Program.Events;

public class UserUpdatedHandler(EntityTrackerDomainService entityTrackerDomainService, TaskExceptionLogger taskExceptionLogger) : IUserUpdatedHandler
{
    public Task UserUpdatedAsync(SocketUser oldUser, SocketUser newUser)
    {
        Task.Run(async () => await taskExceptionLogger.LogOnError(
            entityTrackerDomainService.OnUserUpdatedAsync(oldUser, newUser), nameof(entityTrackerDomainService.OnUserUpdatedAsync)
        ));
        return Task.CompletedTask;
    }
}
