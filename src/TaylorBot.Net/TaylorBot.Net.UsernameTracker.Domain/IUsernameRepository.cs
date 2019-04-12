using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.UsernameTracker.Domain
{
    public interface IUsernameRepository
    {
        Task AddNewUsernameAsync(IUser user);
    }
}
