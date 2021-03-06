using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessagesTracker.Domain;
using TaylorBot.Net.MinutesTracker.Domain;

namespace TaylorBot.Net.StatsTracker.Program.Events
{
    public class ReadyHandler : IShardReadyHandler
    {
        private readonly ILogger<ReadyHandler> _logger;
        private readonly ITaylorBotClient _taylorBotClient;
        private readonly SingletonTaskRunner _minuteSingletonTaskRunner;
        private readonly MinutesTrackerDomainService _minutesTrackerDomainService;
        private readonly SingletonTaskRunner _lastSpokeSingletonTaskRunner;
        private readonly SingletonTaskRunner _channelMessageCountSingletonTaskRunner;
        private readonly MessagesTrackerDomainService _messagesTrackerDomainService;

        public ReadyHandler(
            ILogger<ReadyHandler> logger,
            ITaylorBotClient taylorBotClient,
            SingletonTaskRunner minuteSingletonTaskRunner,
            MinutesTrackerDomainService minutesTrackerDomainService,
            SingletonTaskRunner lastSpokeSingletonTaskRunner,
            SingletonTaskRunner channelMessageCountSingletonTaskRunner,
            MessagesTrackerDomainService messagesTrackerDomainService
        )
        {
            _logger = logger;
            _taylorBotClient = taylorBotClient;
            _minuteSingletonTaskRunner = minuteSingletonTaskRunner;
            _minutesTrackerDomainService = minutesTrackerDomainService;
            _lastSpokeSingletonTaskRunner = lastSpokeSingletonTaskRunner;
            _channelMessageCountSingletonTaskRunner = channelMessageCountSingletonTaskRunner;
            _messagesTrackerDomainService = messagesTrackerDomainService;
        }

        public Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            _ = _minuteSingletonTaskRunner.StartTaskIfNotStarted(
                _minutesTrackerDomainService.StartMinutesAdderAsync,
                nameof(MinutesTrackerDomainService)
            );

            _ = _lastSpokeSingletonTaskRunner.StartTaskIfNotStarted(
                _messagesTrackerDomainService.StartPersistingLastSpokeAsync,
                nameof(MessagesTrackerDomainService.StartPersistingLastSpokeAsync)
            );

            _ = _channelMessageCountSingletonTaskRunner.StartTaskIfNotStarted(
                _messagesTrackerDomainService.StartPersistingTextChannelMessageCountAsync,
                nameof(MessagesTrackerDomainService.StartPersistingTextChannelMessageCountAsync)
            );

            return Task.CompletedTask;
        }
    }
}
