using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MemberLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events
{
    public class GuildUserBanHandler : IGuildUserBannedHandler, IGuildUserUnbannedHandler
    {
        private readonly TaskExceptionLogger taskExceptionLogger;
        private readonly GuildMemberBanLoggerService guildMemberBanLoggerService;

        public GuildUserBanHandler(TaskExceptionLogger taskExceptionLogger, GuildMemberBanLoggerService guildMemberBanLoggerService)
        {
            this.taskExceptionLogger = taskExceptionLogger;
            this.guildMemberBanLoggerService = guildMemberBanLoggerService;
        }

        public Task GuildUserBannedAsync(SocketUser user, SocketGuild guild)
        {
            Task.Run(async () => await taskExceptionLogger.LogOnError(
                guildMemberBanLoggerService.OnGuildMemberBannedAsync(user, guild), nameof(guildMemberBanLoggerService.OnGuildMemberBannedAsync)
            ));
            return Task.CompletedTask;
        }

        public Task GuildUserUnbannedAsync(SocketUser user, SocketGuild guild)
        {
            Task.Run(async () => await taskExceptionLogger.LogOnError(
                guildMemberBanLoggerService.OnGuildMemberUnbannedAsync(user, guild), nameof(guildMemberBanLoggerService.OnGuildMemberUnbannedAsync)
            ));
            return Task.CompletedTask;
        }
    }
}
