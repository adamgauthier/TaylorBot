using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.QuickStart.Domain;

namespace TaylorBot.Net.EntityTracker.Program.Events;

public class QuickStartJoinedGuildHandler : IJoinedGuildHandler
{
    private readonly QuickStartDomainService _quickStartDomainService;
    private readonly TaskExceptionLogger _taskExceptionLogger;

    public QuickStartJoinedGuildHandler(QuickStartDomainService quickStartDomainService, TaskExceptionLogger taskExceptionLogger)
    {
        _quickStartDomainService = quickStartDomainService;
        _taskExceptionLogger = taskExceptionLogger;
    }

    public Task JoinedGuildAsync(SocketGuild guild)
    {
        _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(
            _quickStartDomainService.OnGuildJoinedAsync(guild),
            nameof(QuickStartDomainService)
        ));
        return Task.CompletedTask;
    }
}
