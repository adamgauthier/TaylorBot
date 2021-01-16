using Discord;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.EntityTracker.Domain.User;
using TaylorBot.Net.EntityTracker.Domain.Username;

namespace TaylorBot.Net.EntityTracker.Domain
{
    public class UsernameTrackerDomainService
    {
        private readonly ILogger<UsernameTrackerDomainService> _logger;
        private readonly IUsernameRepository _usernameRepository;

        public UsernameTrackerDomainService(ILogger<UsernameTrackerDomainService> logger, IUsernameRepository usernameRepository)
        {
            _logger = logger;
            _usernameRepository = usernameRepository;
        }

        public async ValueTask AddUsernameAfterUserAddedAsync(IUser user, IUserAddedResult userAddedResult)
        {
            if (userAddedResult.WasAdded)
            {
                _logger.LogInformation(LogString.From($"Added new user {user.FormatLog()}."));
                await _usernameRepository.AddNewUsernameAsync(user);
            }
            else if (userAddedResult.WasUsernameChanged)
            {
                await _usernameRepository.AddNewUsernameAsync(user);
                _logger.LogInformation(LogString.From($"Added new username for {user.FormatLog()}, previously was '{userAddedResult.PreviousUsername}'."));
            }
        }
    }
}
