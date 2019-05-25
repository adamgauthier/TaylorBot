using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using Discord.WebSocket;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.EntityTracker.Program.Events
{
    public class UserUpdatedHandler : IUserUpdatedHandler
    {
        private readonly EntityTrackerDomainService entityTrackerDomainService;
        private readonly TaskExceptionLogger taskExceptionLogger;

        public UserUpdatedHandler(EntityTrackerDomainService entityTrackerDomainService, TaskExceptionLogger taskExceptionLogger)
        {
            this.entityTrackerDomainService = entityTrackerDomainService;
            this.taskExceptionLogger = taskExceptionLogger;
        }

        public Task UserUpdatedAsync(SocketUser oldUser, SocketUser newUser)
        {
            Task.Run(async () => await taskExceptionLogger.LogOnError(
                entityTrackerDomainService.OnUserUpdatedAsync(oldUser, newUser), nameof(entityTrackerDomainService.OnUserUpdatedAsync)
            ));
            return Task.CompletedTask;
        }
    }
}
