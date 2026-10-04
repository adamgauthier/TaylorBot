using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class UserUpdatedHandler(EntityTrackerDomainService entityTrackerDomainService, TaskExceptionLogger taskExceptionLogger, BackgroundTasks backgroundTasks) : IUserUpdatedHandler
{
    public Task UserUpdatedAsync(SocketUser oldUser, SocketUser newUser)
    {
        _ = backgroundTasks.Queue(async () => await taskExceptionLogger.LogOnError(
            entityTrackerDomainService.OnUserUpdatedAsync(oldUser, newUser), nameof(entityTrackerDomainService.OnUserUpdatedAsync)
        ), nameof(UserUpdatedHandler));
        return Task.CompletedTask;
    }
}
