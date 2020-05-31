using System.Threading.Tasks;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.Reminder.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events
{
    public class ReadyHandler : IAllReadyHandler
    {
        private readonly SingletonTaskRunner _birthdaySingletonTaskRunner;
        private readonly SingletonTaskRunner _reminderSingletonTaskRunner;
        private readonly BirthdayRewardNotifierDomainService _birthdayRewardNotifierDomainService;
        private readonly ReminderNotifierDomainService _reminderNotifierDomainService;
        private readonly TaskExceptionLogger _taskExceptionLogger;

        public ReadyHandler(
            SingletonTaskRunner birthdaySingletonTaskRunner,
            SingletonTaskRunner reminderSingletonTaskRunner,
            BirthdayRewardNotifierDomainService birthdayRewardNotifierDomainService,
            ReminderNotifierDomainService reminderNotifierDomainService,
            TaskExceptionLogger taskExceptionLogger
        )
        {
            _birthdaySingletonTaskRunner = birthdaySingletonTaskRunner;
            _reminderSingletonTaskRunner = reminderSingletonTaskRunner;
            _birthdayRewardNotifierDomainService = birthdayRewardNotifierDomainService;
            _reminderNotifierDomainService = reminderNotifierDomainService;
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

            return Task.CompletedTask;
        }
    }
}
