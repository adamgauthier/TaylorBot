using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.Application.Events
{
    public interface IReadyHandler
    {
        Task ReadyAsync(DiscordSocketClient shardClient);
    }
}
