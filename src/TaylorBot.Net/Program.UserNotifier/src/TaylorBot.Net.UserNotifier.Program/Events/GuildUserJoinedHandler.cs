using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.MemberLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class GuildUserJoinedHandler : IGuildUserJoinedHandler
{
    private readonly TaskExceptionLogger taskExceptionLogger;
    private readonly BackgroundTasks backgroundTasks;
    private readonly EntityTrackerDomainService entityTrackerDomainService;

    public GuildUserJoinedHandler(
        TaskExceptionLogger taskExceptionLogger, BackgroundTasks backgroundTasks,
        EntityTrackerDomainService entityTrackerDomainService,
        GuildMemberJoinedLoggerService guildMemberJoinedLoggerService)
    {
        this.taskExceptionLogger = taskExceptionLogger;
        this.backgroundTasks = backgroundTasks;
        this.entityTrackerDomainService = entityTrackerDomainService;

        this.entityTrackerDomainService.GuildMemberFirstJoinedEvent += guildMemberJoinedLoggerService.OnGuildMemberFirstJoinedAsync;
        this.entityTrackerDomainService.GuildMemberRejoinedEvent += guildMemberJoinedLoggerService.OnGuildMemberRejoinedAsync;
    }

    public Task GuildUserJoinedAsync(SocketGuildUser guildUser)
    {
        _ = backgroundTasks.Queue(async () => await taskExceptionLogger.LogOnError(
            entityTrackerDomainService.OnGuildUserJoinedAsync(guildUser), nameof(entityTrackerDomainService.OnGuildUserJoinedAsync)
        ), nameof(GuildUserJoinedHandler));
        return Task.CompletedTask;
    }
}
