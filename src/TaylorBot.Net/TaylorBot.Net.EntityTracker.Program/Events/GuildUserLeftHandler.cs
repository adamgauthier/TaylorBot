using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using Discord.WebSocket;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.EntityTracker.Program.Events
{
    public class GuildUserLeftHandler : IGuildUserLeftHandler
    {
        private readonly TaskExceptionLogger taskExceptionLogger;
        private readonly EntityTrackerDomainService entityTrackerDomainService;

        public GuildUserLeftHandler(TaskExceptionLogger taskExceptionLogger, EntityTrackerDomainService entityTrackerDomainService)
        {
            this.taskExceptionLogger = taskExceptionLogger;
            this.entityTrackerDomainService = entityTrackerDomainService;
        }

        public Task GuildUserLeftAsync(SocketGuildUser guildUser)
        {
            Task.Run(async () => await taskExceptionLogger.LogOnError(
                entityTrackerDomainService.OnGuildUserLeftAsync(guildUser), nameof(entityTrackerDomainService.OnGuildUserLeftAsync)
            ));
            return Task.CompletedTask;
        }
    }
}
