using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.PatreonSync.Domain;
using TaylorBot.Net.Reminder.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events;

public class ReadyHandler : IAllReadyHandler
{
    private readonly SingletonTaskRunner _birthdaySingletonTaskRunner;
    private readonly SingletonTaskRunner _reminderSingletonTaskRunner;
    private readonly SingletonTaskRunner _patreonSyncSingletonTaskRunner;
    private readonly BirthdayRewardNotifierDomainService _birthdayRewardNotifierDomainService;
    private readonly ReminderNotifierDomainService _reminderNotifierDomainService;
    private readonly PatreonSyncDomainService _patreonSyncDomainService;
    private readonly TaskExceptionLogger _taskExceptionLogger;

    public ReadyHandler(
        SingletonTaskRunner birthdaySingletonTaskRunner,
        SingletonTaskRunner reminderSingletonTaskRunner,
        SingletonTaskRunner patreonSyncSingletonTaskRunner,
        BirthdayRewardNotifierDomainService birthdayRewardNotifierDomainService,
        ReminderNotifierDomainService reminderNotifierDomainService,
        PatreonSyncDomainService patreonSyncDomainService,
        TaskExceptionLogger taskExceptionLogger
    )
    {
        _birthdaySingletonTaskRunner = birthdaySingletonTaskRunner;
        _reminderSingletonTaskRunner = reminderSingletonTaskRunner;
        _patreonSyncSingletonTaskRunner = patreonSyncSingletonTaskRunner;
        _birthdayRewardNotifierDomainService = birthdayRewardNotifierDomainService;
        _reminderNotifierDomainService = reminderNotifierDomainService;
        _patreonSyncDomainService = patreonSyncDomainService;
        _taskExceptionLogger = taskExceptionLogger;
    }

    public Task AllShardsReadyAsync()
    {
        _ = _birthdaySingletonTaskRunner.StartTaskIfNotStarted(
            _birthdayRewardNotifierDomainService.StartCheckingBirthdaysAsync,
            nameof(BirthdayRewardNotifierDomainService)
        );

        _ = _reminderSingletonTaskRunner.StartTaskIfNotStarted(
            _reminderNotifierDomainService.StartCheckingRemindersAsync,
            nameof(ReminderNotifierDomainService)
        );

        _ = _patreonSyncSingletonTaskRunner.StartTaskIfNotStarted(
            _patreonSyncDomainService.StartSyncingPatreonSupportersAsync,
            nameof(PatreonSyncDomainService)
        );

        return Task.CompletedTask;
    }
}
