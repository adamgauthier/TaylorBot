using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.EntityTracker.Domain.User
{
    public interface IUserRepository
    {
        ValueTask<UserAddedResult> AddNewUserAsync(IUser user);
    }
}
