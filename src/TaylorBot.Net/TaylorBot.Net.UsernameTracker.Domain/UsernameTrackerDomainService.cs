using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.UsernameTracker.Domain
{
    public class UsernameTrackerDomainService
    {
        private readonly ILogger<UsernameTrackerDomainService> logger;
        private readonly IUsernameRepository usernameRepository;

        public UsernameTrackerDomainService(ILogger<UsernameTrackerDomainService> logger, IUsernameRepository usernameRepository)
        {
            this.logger = logger;
            this.usernameRepository = usernameRepository;
        }

        public async Task OnUsernameUpdatedAsync(SocketUser oldUser, SocketUser newUser)
        {
            await usernameRepository.AddNewUsernameAsync(newUser);
            logger.LogInformation(LogString.From($"Added new username for {newUser.FormatLog()}, previously was '{oldUser.Username}'."));
        }
    }
}
