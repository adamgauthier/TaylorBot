using Discord.WebSocket;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;

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

        public Task GuildUserLeftAsync(SocketGuild guild, SocketUser user)
        {
            Task.Run(async () => await taskExceptionLogger.LogOnError(
                entityTrackerDomainService.OnGuildUserLeftAsync(guild, user),
                nameof(entityTrackerDomainService.OnGuildUserLeftAsync)
            ));
            return Task.CompletedTask;
        }
    }
}
