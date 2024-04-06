using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.EntityTracker.Domain.User;

public interface IUserRepository
{
    ValueTask<UserAddedResult> AddNewUserAsync(DiscordUser user);
}
