using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using Discord.WebSocket;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.EntityTracker.Program.Events
{
    public class ShardReadyHandler : IShardReadyHandler
    {
        private readonly EntityTrackerDomainService entityTrackerDomainService;
        private readonly TaskExceptionLogger taskExceptionLogger;

        public ShardReadyHandler(EntityTrackerDomainService entityTrackerDomainService, TaskExceptionLogger taskExceptionLogger)
        {
            this.entityTrackerDomainService = entityTrackerDomainService;
            this.taskExceptionLogger = taskExceptionLogger;
        }

        public Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            Task.Run(async () => await taskExceptionLogger.LogOnError(
                entityTrackerDomainService.OnShardReadyAsync(shardClient), nameof(entityTrackerDomainService.OnShardReadyAsync)
            ));
            return Task.CompletedTask;
        }
    }
}
