using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IShardReadyHandler
    {
        Task ShardReadyAsync(DiscordSocketClient shardClient);
    }
}
