using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using Discord.WebSocket;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.EntityTracker.Program.Events
{
    public class UsernameJoinedGuildHandler : IJoinedGuildHandler
    {
        private readonly EntityTrackerDomainService entityTrackerDomainService;
        private readonly TaskExceptionLogger taskExceptionLogger;

        public UsernameJoinedGuildHandler(EntityTrackerDomainService entityTrackerDomainService, TaskExceptionLogger taskExceptionLogger)
        {
            this.entityTrackerDomainService = entityTrackerDomainService;
            this.taskExceptionLogger = taskExceptionLogger;
        }

        public Task JoinedGuildAsync(SocketGuild guild)
        {
            Task.Run(async () => await taskExceptionLogger.LogOnError(
                entityTrackerDomainService.OnGuildJoinedAsync(guild), nameof(entityTrackerDomainService.OnGuildJoinedAsync)
            ));
            return Task.CompletedTask;
        }
    }
}
