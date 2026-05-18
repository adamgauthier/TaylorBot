using Discord.WebSocket;
using Microsoft.Extensions.Options;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.MessagesTracker.Domain;
using TaylorBot.Net.MinutesTracker.Domain;
using TaylorBot.Net.PatreonSync.Domain;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.Reminder.Domain;
using TaylorBot.Net.TumblrNotifier.Domain;
using TaylorBot.Net.UserNotifier.Program.Options;
using TaylorBot.Net.YoutubeNotifier.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class ShardReadyHandler(
    SingletonTaskRunner redditSingletonTaskRunner,
    SingletonTaskRunner youtubeSingletonTaskRunner,
    SingletonTaskRunner tumblrSingletonTaskRunner,
    RedditNotifierService redditNotiferService,
    YoutubeNotifierService youtubeNotiferService,
    TumblrNotifierService tumblrNotifierService,
    SingletonTaskRunner birthdayCalendarSingletonTaskRunner,
    BirthdayCalendarDomainService birthdayCalendarDomainService,
    SingletonTaskRunner reminderSingletonTaskRunner,
    ReminderNotifierDomainService reminderNotifierDomainService,
    SingletonTaskRunner patreonSyncSingletonTaskRunner,
    PatreonSyncDomainService patreonSyncDomainService,
    SingletonTaskRunner birthdayRoleAddSingletonTaskRunner,
    SingletonTaskRunner birthdayRoleRemoveSingletonTaskRunner,
    BirthdayRoleDomainService birthdayRoleDomainService,
    SingletonTaskRunner birthdaySingletonTaskRunner,
    BirthdayRewardNotifierDomainService birthdayRewardNotifierDomainService,
    SingletonTaskRunner minuteSingletonTaskRunner,
    MinutesTrackerDomainService minutesTrackerDomainService,
    SingletonTaskRunner lastSpokeSingletonTaskRunner,
    SingletonTaskRunner channelMessageCountSingletonTaskRunner,
    SingletonTaskRunner memberMessageAndWordsSingletonTaskRunner,
    MessagesTrackerDomainService messagesTrackerDomainService,
    EntityTrackerDomainService entityTrackerDomainService,
    IOptionsMonitor<UserNotifierStartupOptions> optionsMonitor,
    TaskExceptionLogger taskExceptionLogger
    ) : IShardReadyHandler
{
    public Task ShardReadyAsync(DiscordSocketClient shardClient)
    {
        var options = optionsMonitor.CurrentValue;

        // Keep tracking/persistence startup immediate so Discord events are captured and flushed right away.
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

        _ = redditSingletonTaskRunner.RunTaskIfNotRan(
            () => RunAfterDelayAsync(redditNotiferService.StartCheckingRedditsAsync, options.RedditInitialDelay),
            nameof(RedditNotifierService)
        );

        _ = youtubeSingletonTaskRunner.RunTaskIfNotRan(
            () => RunAfterDelayAsync(youtubeNotiferService.StartCheckingYoutubesAsync, options.YoutubeInitialDelay),
            nameof(YoutubeNotifierService)
        );

        _ = tumblrSingletonTaskRunner.RunTaskIfNotRan(
            () => RunAfterDelayAsync(tumblrNotifierService.StartCheckingTumblrsAsync, options.TumblrInitialDelay),
            nameof(TumblrNotifierService)
        );

        _ = birthdayCalendarSingletonTaskRunner.RunTaskIfNotRan(
            () => RunAfterDelayAsync(birthdayCalendarDomainService.StartRefreshingBirthdayCalendarAsync, options.BirthdayCalendarInitialDelay),
            nameof(birthdayCalendarDomainService.StartRefreshingBirthdayCalendarAsync)
        );

        _ = reminderSingletonTaskRunner.RunTaskIfNotRan(
            () => RunAfterDelayAsync(reminderNotifierDomainService.StartCheckingRemindersAsync, options.ReminderInitialDelay),
            nameof(ReminderNotifierDomainService)
        );

        _ = patreonSyncSingletonTaskRunner.RunTaskIfNotRan(
            () => RunAfterDelayAsync(patreonSyncDomainService.StartSyncingPatreonSupportersAsync, options.PatreonSyncInitialDelay),
            nameof(PatreonSyncDomainService)
        );

        _ = birthdayRoleAddSingletonTaskRunner.RunTaskIfNotRan(
            () => RunAfterDelayAsync(birthdayRoleDomainService.StartAddingBirthdayRolesAsync, options.BirthdayRoleAddInitialDelay),
            nameof(birthdayRoleDomainService.StartAddingBirthdayRolesAsync)
        );

        _ = birthdayRoleRemoveSingletonTaskRunner.RunTaskIfNotRan(
            () => RunAfterDelayAsync(birthdayRoleDomainService.StartRemovingBirthdayRolesAsync, options.BirthdayRoleRemoveInitialDelay),
            nameof(birthdayRoleDomainService.StartRemovingBirthdayRolesAsync)
        );

        _ = birthdaySingletonTaskRunner.RunTaskIfNotRan(
            () => RunAfterDelayAsync(birthdayRewardNotifierDomainService.StartCheckingBirthdaysAsync, options.BirthdayRewardInitialDelay),
            nameof(BirthdayRewardNotifierDomainService)
        );

        return Task.CompletedTask;
    }

    private static async Task RunAfterDelayAsync(Func<Task> action, TimeSpan delay)
    {
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay);
        }

        await action();
    }
}
