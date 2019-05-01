using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using Discord.WebSocket;
using TaylorBot.Net.QuickStart.Domain;

namespace TaylorBot.Net.UserTracker.Program.Events
{
    public class JoinedGuildHandler : IJoinedGuildHandler
    {
        private readonly QuickStartDomainService quickStartDomainService;

        public JoinedGuildHandler(QuickStartDomainService quickStartDomainService)
        {
            this.quickStartDomainService = quickStartDomainService;
        }

        public async Task JoinedGuildAsync(SocketGuild guild)
        {
            await quickStartDomainService.OnGuildJoinedAsync(guild);
        }
    }
}
