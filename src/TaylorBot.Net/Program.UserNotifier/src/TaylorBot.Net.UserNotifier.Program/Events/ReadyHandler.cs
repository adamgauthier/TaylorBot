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
    SingletonTaskRunner birthdayRoleAddSingletonTaskRunner,
    SingletonTaskRunner birthdayRoleRemoveSingletonTaskRunner,
    BirthdayRewardNotifierDomainService birthdayRewardNotifierDomainService,
    ReminderNotifierDomainService reminderNotifierDomainService,
    PatreonSyncDomainService patreonSyncDomainService,
    BirthdayRoleDomainService birthdayRoleDomainService
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

        _ = birthdayRoleAddSingletonTaskRunner.StartTaskIfNotStarted(
            birthdayRoleDomainService.StartAddingBirthdayRolesAsync,
            nameof(birthdayRoleDomainService.StartAddingBirthdayRolesAsync)
        );

        _ = birthdayRoleRemoveSingletonTaskRunner.StartTaskIfNotStarted(
            birthdayRoleDomainService.StartRemovingBirthdayRolesAsync,
            nameof(birthdayRoleDomainService.StartRemovingBirthdayRolesAsync)
        );

        return Task.CompletedTask;
    }
}
