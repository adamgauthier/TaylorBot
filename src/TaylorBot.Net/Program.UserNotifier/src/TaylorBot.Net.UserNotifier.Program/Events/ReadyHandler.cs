using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.PatreonSync.Domain;
using TaylorBot.Net.Reminder.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class ReadyHandler(
    SingletonTaskRunner birthdaySingletonTaskRunner,
    SingletonTaskRunner reminderSingletonTaskRunner,
    SingletonTaskRunner patreonSyncSingletonTaskRunner,
    BirthdayRewardNotifierDomainService birthdayRewardNotifierDomainService,
    ReminderNotifierDomainService reminderNotifierDomainService,
    PatreonSyncDomainService patreonSyncDomainService
    ) : IAllReadyHandler
{
    public Task AllShardsReadyAsync()
    {
        _ = birthdaySingletonTaskRunner.StartTaskIfNotStarted(
            birthdayRewardNotifierDomainService.StartCheckingBirthdaysAsync,
            nameof(BirthdayRewardNotifierDomainService)
        );

        _ = reminderSingletonTaskRunner.StartTaskIfNotStarted(
            reminderNotifierDomainService.StartCheckingRemindersAsync,
            nameof(ReminderNotifierDomainService)
        );

        _ = patreonSyncSingletonTaskRunner.StartTaskIfNotStarted(
            patreonSyncDomainService.StartSyncingPatreonSupportersAsync,
            nameof(PatreonSyncDomainService)
        );

        return Task.CompletedTask;
    }
}
