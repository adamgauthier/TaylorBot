using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.EntityTracker.Domain.Username;

public interface IUsernameRepository
{
    ValueTask AddNewUsernameAsync(DiscordUser user);
}
