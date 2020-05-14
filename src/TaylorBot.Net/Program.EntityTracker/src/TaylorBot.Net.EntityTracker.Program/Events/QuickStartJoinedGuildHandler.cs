using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using Discord.WebSocket;
using TaylorBot.Net.QuickStart.Domain;

namespace TaylorBot.Net.EntityTracker.Program.Events
{
    public class QuickStartJoinedGuildHandler : IJoinedGuildHandler
    {
        private readonly QuickStartDomainService quickStartDomainService;

        public QuickStartJoinedGuildHandler(QuickStartDomainService quickStartDomainService)
        {
            this.quickStartDomainService = quickStartDomainService;
        }

        public Task JoinedGuildAsync(SocketGuild guild)
        {
            Task.Run(async () => await quickStartDomainService.OnGuildJoinedAsync(guild));
            return Task.CompletedTask;
        }
    }
}
