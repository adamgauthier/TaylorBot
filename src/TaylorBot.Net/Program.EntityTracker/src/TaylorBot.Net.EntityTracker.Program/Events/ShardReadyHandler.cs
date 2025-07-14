using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.MessagesTracker.Domain;
using TaylorBot.Net.MinutesTracker.Domain;

namespace TaylorBot.Net.EntityTracker.Program.Events;

public class ShardReadyHandler(
    SingletonTaskRunner minuteSingletonTaskRunner,
    MinutesTrackerDomainService minutesTrackerDomainService,
    SingletonTaskRunner lastSpokeSingletonTaskRunner,
    SingletonTaskRunner channelMessageCountSingletonTaskRunner,
    SingletonTaskRunner memberMessageAndWordsSingletonTaskRunner,
    MessagesTrackerDomainService messagesTrackerDomainService,
    EntityTrackerDomainService entityTrackerDomainService,
    TaskExceptionLogger taskExceptionLogger
    ) : IShardReadyHandler
{
    public Task ShardReadyAsync(DiscordSocketClient shardClient)
    {
        _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
            entityTrackerDomainService.OnShardReadyAsync(shardClient), nameof(entityTrackerDomainService.OnShardReadyAsync)
        ));

        _ = minuteSingletonTaskRunner.RunTaskIfNotRan(
            minutesTrackerDomainService.StartMinutesAdderAsync,
            nameof(MinutesTrackerDomainService)
        );

        _ = lastSpokeSingletonTaskRunner.RunTaskIfNotRan(
            messagesTrackerDomainService.StartPersistingLastSpokeAsync,
            nameof(MessagesTrackerDomainService.StartPersistingLastSpokeAsync)
        );

        _ = channelMessageCountSingletonTaskRunner.RunTaskIfNotRan(
            messagesTrackerDomainService.StartPersistingTextChannelMessageCountAsync,
            nameof(MessagesTrackerDomainService.StartPersistingTextChannelMessageCountAsync)
        );

        _ = memberMessageAndWordsSingletonTaskRunner.RunTaskIfNotRan(
            messagesTrackerDomainService.StartPersistingMemberMessagesAndWordsAsync,
            nameof(MessagesTrackerDomainService.StartPersistingMemberMessagesAndWordsAsync)
        );

        return Task.CompletedTask;
    }
}
