using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.MinutesTracker.Domain;
using Discord.WebSocket;

namespace TaylorBot.Net.StatsTracker.Program.Events
{
    public class ReadyHandler : IShardReadyHandler
    {
        private readonly TaylorBotClient taylorBotClient;
        private readonly SingletonTaskRunner minuteSingletonTaskRunner;
        private readonly MinutesTrackerDomainService minutesTrackerDomainService;

        public ReadyHandler(
            TaylorBotClient taylorBotClient,
            SingletonTaskRunner minuteSingletonTaskRunner,
            MinutesTrackerDomainService minutesTrackerDomainService)
        {
            this.taylorBotClient = taylorBotClient;
            this.minuteSingletonTaskRunner = minuteSingletonTaskRunner;
            this.minutesTrackerDomainService = minutesTrackerDomainService;
        }

        public Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            minuteSingletonTaskRunner.StartTaskIfNotStarted(async () =>
            {
                await minutesTrackerDomainService.StartMinutesAdderAsync();
            });

            return Task.CompletedTask;
        }
    }
}
