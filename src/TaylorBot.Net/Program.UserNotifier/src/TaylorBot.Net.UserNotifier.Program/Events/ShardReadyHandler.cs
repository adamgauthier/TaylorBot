using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class ShardReadyHandler(
    UserNotifierJobs jobs,
    EntityTrackerDomainService entityTracker,
    BackgroundTasks backgroundTasks,
    TaskExceptionLogger taskExceptionLogger) : IShardReadyHandler
{
    public Task ShardReadyAsync(DiscordSocketClient shardClient)
    {
        _ = backgroundTasks.Queue(
            () => taskExceptionLogger.LogOnError(entityTracker.OnShardReadyAsync(shardClient), nameof(entityTracker.OnShardReadyAsync)),
            nameof(entityTracker.OnShardReadyAsync));
        jobs.StartAfterReady();
        return Task.CompletedTask;
    }
}
