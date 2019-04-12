using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IUserUpdatedHandler
    {
        Task UserUpdatedAsync(SocketUser oldUser, SocketUser newUser);
    }
}
