using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MemberLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class GuildUserBanHandler(TaskExceptionLogger taskExceptionLogger, BackgroundTasks backgroundTasks, GuildMemberBanLoggerService guildMemberBanLoggerService) : IGuildUserBannedHandler, IGuildUserUnbannedHandler
{
    public Task GuildUserBannedAsync(SocketUser user, SocketGuild guild)
    {
        _ = backgroundTasks.Queue(async () => await taskExceptionLogger.LogOnError(
            guildMemberBanLoggerService.OnGuildMemberBannedAsync(user, guild), nameof(guildMemberBanLoggerService.OnGuildMemberBannedAsync)
        ), nameof(GuildUserBanHandler));
        return Task.CompletedTask;
    }

    public Task GuildUserUnbannedAsync(SocketUser user, SocketGuild guild)
    {
        _ = backgroundTasks.Queue(async () => await taskExceptionLogger.LogOnError(
            guildMemberBanLoggerService.OnGuildMemberUnbannedAsync(user, guild), nameof(guildMemberBanLoggerService.OnGuildMemberUnbannedAsync)
        ), nameof(GuildUserBanHandler));
        return Task.CompletedTask;
    }
}
