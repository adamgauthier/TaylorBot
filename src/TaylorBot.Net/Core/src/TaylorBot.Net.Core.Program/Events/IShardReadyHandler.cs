using Discord.WebSocket;

namespace TaylorBot.Net.Core.Program.Events;

public interface IShardReadyHandler
{
    Task ShardReadyAsync(DiscordSocketClient shardClient);
}
