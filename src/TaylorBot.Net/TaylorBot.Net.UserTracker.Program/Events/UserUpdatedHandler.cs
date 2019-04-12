using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using Discord.WebSocket;
using TaylorBot.Net.UsernameTracker.Domain;

namespace TaylorBot.Net.UserTracker.Program.Events
{
    public class UserUpdatedHandler : IUserUpdatedHandler
    {
        private readonly UsernameTrackerDomainService usernameTrackerDomainService;

        public UserUpdatedHandler(UsernameTrackerDomainService usernameTrackerDomainService)
        {
            this.usernameTrackerDomainService = usernameTrackerDomainService;
        }

        public async Task UserUpdatedAsync(SocketUser oldUser, SocketUser newUser)
        {
            if (oldUser.Username != newUser.Username)
            {
                await usernameTrackerDomainService.OnUsernameUpdatedAsync(oldUser, newUser);
            }
        }
    }
}
