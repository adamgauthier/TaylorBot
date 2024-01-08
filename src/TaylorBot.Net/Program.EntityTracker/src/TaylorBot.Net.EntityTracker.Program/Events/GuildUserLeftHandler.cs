using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.EntityTracker.Program.Events;

public class GuildUserLeftHandler(TaskExceptionLogger taskExceptionLogger, EntityTrackerDomainService entityTrackerDomainService) : IGuildUserLeftHandler
{
    public Task GuildUserLeftAsync(SocketGuild guild, SocketUser user)
    {
        Task.Run(async () => await taskExceptionLogger.LogOnError(
            entityTrackerDomainService.OnGuildUserLeftAsync(guild, user),
            nameof(entityTrackerDomainService.OnGuildUserLeftAsync)
        ));
        return Task.CompletedTask;
    }
}
