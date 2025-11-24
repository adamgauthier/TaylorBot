using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.User;
using TaylorBot.Net.EntityTracker.Domain.User;
using TaylorBot.Net.EntityTracker.Domain.Username;

namespace TaylorBot.Net.EntityTracker.Domain;

public partial class UsernameTrackerDomainService(ILogger<UsernameTrackerDomainService> logger, IUsernameRepository usernameRepository)
{
    public async ValueTask AddUsernameAfterUserAddedAsync(DiscordUser user, IUserAddedResult userAddedResult)
    {
        if (userAddedResult.WasAdded)
        {
            LogAddedNewUser(user.FormatLog());
            await usernameRepository.AddNewUsernameAsync(user);
        }
        else if (userAddedResult.WasUsernameChanged)
        {
            await usernameRepository.AddNewUsernameAsync(user);
            LogAddedNewUsername(user.FormatLog(), userAddedResult.PreviousUsername);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Added new user {User}.")]
    private partial void LogAddedNewUser(string user);

    [LoggerMessage(Level = LogLevel.Information, Message = "Added new username for {User}, previously was '{PreviousUsername}'.")]
    private partial void LogAddedNewUsername(string user, string? previousUsername);
}
