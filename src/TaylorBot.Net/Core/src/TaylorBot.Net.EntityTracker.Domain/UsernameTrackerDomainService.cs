using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.EntityTracker.Domain.User;
using TaylorBot.Net.EntityTracker.Domain.Username;

namespace TaylorBot.Net.EntityTracker.Domain;

public class UsernameTrackerDomainService(ILogger<UsernameTrackerDomainService> logger, IUsernameRepository usernameRepository)
{
    public async ValueTask AddUsernameAfterUserAddedAsync(IUser user, IUserAddedResult userAddedResult)
    {
        if (userAddedResult.WasAdded)
        {
            logger.LogInformation($"Added new user {user.FormatLog()}.");
            await usernameRepository.AddNewUsernameAsync(user);
        }
        else if (userAddedResult.WasUsernameChanged)
        {
            await usernameRepository.AddNewUsernameAsync(user);
            logger.LogInformation($"Added new username for {user.FormatLog()}, previously was '{userAddedResult.PreviousUsername}'.");
        }
    }
}
