using Discord;

namespace TaylorBot.Net.EntityTracker.Domain.User;

public interface IUserRepository
{
    ValueTask<UserAddedResult> AddNewUserAsync(IUser user);
}
