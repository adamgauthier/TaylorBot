using Discord.WebSocket;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.PatreonSync.Domain;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.Reminder.Domain;
using TaylorBot.Net.TumblrNotifier.Domain;
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
    BirthdayRewardNotifierDomainService birthdayRewardNotifierDomainService
    ) : IShardReadyHandler
{
    public Task ShardReadyAsync(DiscordSocketClient shardClient)
    {
        _ = redditSingletonTaskRunner.StartTaskIfNotStarted(
            redditNotiferService.StartCheckingRedditsAsync,
            nameof(RedditNotifierService)
        );

        _ = youtubeSingletonTaskRunner.StartTaskIfNotStarted(
            youtubeNotiferService.StartCheckingYoutubesAsync,
            nameof(YoutubeNotifierService)
        );

        _ = tumblrSingletonTaskRunner.StartTaskIfNotStarted(
            tumblrNotifierService.StartCheckingTumblrsAsync,
            nameof(TumblrNotifierService)
        );

        _ = birthdayCalendarSingletonTaskRunner.StartTaskIfNotStarted(
            birthdayCalendarDomainService.StartRefreshingBirthdayCalendarAsync,
            nameof(birthdayCalendarDomainService.StartRefreshingBirthdayCalendarAsync)
        );

        _ = reminderSingletonTaskRunner.StartTaskIfNotStarted(
            reminderNotifierDomainService.StartCheckingRemindersAsync,
            nameof(ReminderNotifierDomainService)
        );

        _ = patreonSyncSingletonTaskRunner.StartTaskIfNotStarted(
            patreonSyncDomainService.StartSyncingPatreonSupportersAsync,
            nameof(PatreonSyncDomainService)
        );

        _ = birthdayRoleAddSingletonTaskRunner.StartTaskIfNotStarted(
            birthdayRoleDomainService.StartAddingBirthdayRolesAsync,
            nameof(birthdayRoleDomainService.StartAddingBirthdayRolesAsync)
        );

        _ = birthdayRoleRemoveSingletonTaskRunner.StartTaskIfNotStarted(
            birthdayRoleDomainService.StartRemovingBirthdayRolesAsync,
            nameof(birthdayRoleDomainService.StartRemovingBirthdayRolesAsync)
        );

        _ = birthdaySingletonTaskRunner.StartTaskIfNotStarted(
            birthdayRewardNotifierDomainService.StartCheckingBirthdaysAsync,
            nameof(BirthdayRewardNotifierDomainService)
        );

        return Task.CompletedTask;
    }
}
