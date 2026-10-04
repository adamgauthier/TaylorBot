using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.QuickStart.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class QuickStartJoinedGuildHandler(QuickStartDomainService quickStartDomainService, TaskExceptionLogger taskExceptionLogger, BackgroundTasks backgroundTasks) : IJoinedGuildHandler
{
    public Task JoinedGuildAsync(SocketGuild guild)
    {
        _ = backgroundTasks.Queue(async () => await taskExceptionLogger.LogOnError(
            quickStartDomainService.OnGuildJoinedAsync(guild),
            nameof(QuickStartDomainService)
        ), nameof(QuickStartJoinedGuildHandler));
        return Task.CompletedTask;
    }
}
