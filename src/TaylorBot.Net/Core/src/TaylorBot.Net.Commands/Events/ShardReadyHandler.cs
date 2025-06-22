using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Events;

public class ShardReadyHandler(SingletonTaskRunner commandMentionCacheRunner, IApplicationCommandsRepository commandRepository) : IShardReadyHandler
{
    public Task ShardReadyAsync(DiscordSocketClient shardClient)
    {
        // Cache command ids for mentions
        _ = commandMentionCacheRunner.StartTaskIfNotStarted(
            commandRepository.CacheCommandsAsync,
            nameof(IApplicationCommandsRepository.CacheCommandsAsync)
        );
        return Task.CompletedTask;
    }
}
