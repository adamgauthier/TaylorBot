using Discord.WebSocket;
using TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2026.Domain;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Events;

public class ValentineGiveawayReadyHandler(
    SingletonTaskRunner giveawaySingletonTaskRunner,
    ValentineGiveawayDomainService valentineGiveawayDomainService) : IShardReadyHandler
{
    public Task ShardReadyAsync(DiscordSocketClient shardClient)
    {
        _ = giveawaySingletonTaskRunner.RunTaskIfNotRan(
            valentineGiveawayDomainService.StartGiveawayAsync,
            nameof(ValentineGiveawayDomainService.StartGiveawayAsync)
        );
        return Task.CompletedTask;
    }
}
