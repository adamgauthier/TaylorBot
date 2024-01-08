using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MemberLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class GuildUserLeftHandler(TaskExceptionLogger taskExceptionLogger, GuildMemberLeftLoggerService guildMemberLeftLoggerService) : IGuildUserLeftHandler
{
    public Task GuildUserLeftAsync(SocketGuild guild, SocketUser user)
    {
        Task.Run(async () => await taskExceptionLogger.LogOnError(
            guildMemberLeftLoggerService.OnGuildMemberLeftAsync(guild, user),
            nameof(guildMemberLeftLoggerService.OnGuildMemberLeftAsync)
        ));
        return Task.CompletedTask;
    }
}
