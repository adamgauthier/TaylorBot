using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MinutesTracker.Domain;

namespace TaylorBot.Net.StatsTracker.Program.Events
{
    public class ReadyHandler : IShardReadyHandler
    {
        private readonly ILogger<ReadyHandler> _logger;
        private readonly ITaylorBotClient _taylorBotClient;
        private readonly SingletonTaskRunner _minuteSingletonTaskRunner;
        private readonly MinutesTrackerDomainService _minutesTrackerDomainService;

        public ReadyHandler(
            ILogger<ReadyHandler> logger,
            ITaylorBotClient taylorBotClient,
            SingletonTaskRunner minuteSingletonTaskRunner,
            MinutesTrackerDomainService minutesTrackerDomainService)
        {
            _logger = logger;
            _taylorBotClient = taylorBotClient;
            _minuteSingletonTaskRunner = minuteSingletonTaskRunner;
            _minutesTrackerDomainService = minutesTrackerDomainService;
        }

        public Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            _ = _minuteSingletonTaskRunner.StartTaskIfNotStarted(
                _minutesTrackerDomainService.StartMinutesAdderAsync,
                nameof(MinutesTrackerDomainService)
            );

            return Task.CompletedTask;
        }
    }
}
