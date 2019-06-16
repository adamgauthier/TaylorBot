using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using Discord.WebSocket;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.ChannelLogging.Domain;

namespace TaylorBot.Net.EntityTracker.Program.Events
{
    public class GuildUserJoinedHandler : IGuildUserJoinedHandler
    {
        private readonly TaskExceptionLogger taskExceptionLogger;
        private readonly EntityTrackerDomainService entityTrackerDomainService;

        public GuildUserJoinedHandler(
            TaskExceptionLogger taskExceptionLogger,
            EntityTrackerDomainService entityTrackerDomainService,
            GuildMemberJoinedLoggerService guildMemberJoinedLoggerService)
        {
            this.taskExceptionLogger = taskExceptionLogger;
            this.entityTrackerDomainService = entityTrackerDomainService;

            this.entityTrackerDomainService.GuildMemberFirstJoinedEvent += guildMemberJoinedLoggerService.OnGuildMemberFirstJoinedAsync;
            this.entityTrackerDomainService.GuildMemberRejoinedEvent += guildMemberJoinedLoggerService.OnGuildMemberRejoinedAsync;
        }

        public Task GuildUserJoinedAsync(SocketGuildUser guildUser)
        {
            Task.Run(async () => await taskExceptionLogger.LogOnError(
                entityTrackerDomainService.OnGuildUserJoinedAsync(guildUser), nameof(entityTrackerDomainService.OnGuildUserJoinedAsync)
            ));
            return Task.CompletedTask;
        }
    }
}
