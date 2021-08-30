using Discord.WebSocket;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.MessagesTracker.Domain;
using TaylorBot.Net.MinutesTracker.Domain;

namespace TaylorBot.Net.EntityTracker.Program.Events
{
    public class ShardReadyHandler : IShardReadyHandler
    {
        private readonly SingletonTaskRunner _minuteSingletonTaskRunner;
        private readonly MinutesTrackerDomainService _minutesTrackerDomainService;
        private readonly SingletonTaskRunner _lastSpokeSingletonTaskRunner;
        private readonly SingletonTaskRunner _channelMessageCountSingletonTaskRunner;
        private readonly SingletonTaskRunner _memberMessageAndWordsSingletonTaskRunner;
        private readonly MessagesTrackerDomainService _messagesTrackerDomainService;
        private readonly EntityTrackerDomainService _entityTrackerDomainService;
        private readonly TaskExceptionLogger _taskExceptionLogger;

        public ShardReadyHandler(
            SingletonTaskRunner minuteSingletonTaskRunner,
            MinutesTrackerDomainService minutesTrackerDomainService,
            SingletonTaskRunner lastSpokeSingletonTaskRunner,
            SingletonTaskRunner channelMessageCountSingletonTaskRunner,
            SingletonTaskRunner memberMessageAndWordsSingletonTaskRunner,
            MessagesTrackerDomainService messagesTrackerDomainService,
            EntityTrackerDomainService entityTrackerDomainService,
            TaskExceptionLogger taskExceptionLogger
        )
        {
            _minuteSingletonTaskRunner = minuteSingletonTaskRunner;
            _minutesTrackerDomainService = minutesTrackerDomainService;
            _lastSpokeSingletonTaskRunner = lastSpokeSingletonTaskRunner;
            _channelMessageCountSingletonTaskRunner = channelMessageCountSingletonTaskRunner;
            _memberMessageAndWordsSingletonTaskRunner = memberMessageAndWordsSingletonTaskRunner;
            _messagesTrackerDomainService = messagesTrackerDomainService;
            _entityTrackerDomainService = entityTrackerDomainService;
            _taskExceptionLogger = taskExceptionLogger;
        }

        public Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(
                _entityTrackerDomainService.OnShardReadyAsync(shardClient), nameof(_entityTrackerDomainService.OnShardReadyAsync)
            ));

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

            _ = _memberMessageAndWordsSingletonTaskRunner.StartTaskIfNotStarted(
                _messagesTrackerDomainService.StartPersistingMemberMessagesAndWordsAsync,
                nameof(MessagesTrackerDomainService.StartPersistingMemberMessagesAndWordsAsync)
            );

            return Task.CompletedTask;
        }
    }
}
