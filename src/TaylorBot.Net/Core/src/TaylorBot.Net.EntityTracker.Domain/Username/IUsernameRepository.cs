using Discord;

namespace TaylorBot.Net.EntityTracker.Domain.Username;

public interface IUsernameRepository
{
    ValueTask AddNewUsernameAsync(IUser user);
}
