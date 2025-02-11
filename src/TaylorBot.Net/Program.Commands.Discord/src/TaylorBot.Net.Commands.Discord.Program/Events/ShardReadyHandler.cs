using Discord.WebSocket;
using TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2025.Domain;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Events;

public class ShardReadyHandler(SingletonTaskRunner valentineGiveawaySingletonTaskRunner, ValentineGiveawayDomainService valentineGiveawayDomainService) : IShardReadyHandler
{
    public Task ShardReadyAsync(DiscordSocketClient shardClient)
    {
        _ = valentineGiveawaySingletonTaskRunner.StartTaskIfNotStarted(
            valentineGiveawayDomainService.StartGiveawayAsync,
            nameof(ValentineGiveawayDomainService)
        );

        return Task.CompletedTask;
    }
}
