using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.EntityTracker.Domain.Username
{
    public interface IUsernameRepository
    {
        Task AddNewUsernameAsync(IUser user);
        Task<string> GetLatestUsernameAsync(IUser user);
    }
}
