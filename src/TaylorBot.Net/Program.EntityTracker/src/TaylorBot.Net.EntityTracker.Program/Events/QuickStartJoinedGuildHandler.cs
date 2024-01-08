using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.QuickStart.Domain;

namespace TaylorBot.Net.EntityTracker.Program.Events;

public class QuickStartJoinedGuildHandler(QuickStartDomainService quickStartDomainService, TaskExceptionLogger taskExceptionLogger) : IJoinedGuildHandler
{
    public Task JoinedGuildAsync(SocketGuild guild)
    {
        _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
            quickStartDomainService.OnGuildJoinedAsync(guild),
            nameof(QuickStartDomainService)
        ));
        return Task.CompletedTask;
    }
}
