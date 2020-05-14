using System.Threading.Tasks;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.Reminder.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events
{
    public class ReadyHandler : IAllReadyHandler
    {
        private readonly SingletonTaskRunner birthdaySingletonTaskRunner;
        private readonly SingletonTaskRunner reminderSingletonTaskRunner;
        private readonly BirthdayRewardNotifierDomainService birthdayRewardNotifierDomainService;
        private readonly ReminderNotifierDomainService reminderNotifierDomainService;

        public ReadyHandler(
            SingletonTaskRunner birthdaySingletonTaskRunner,
            SingletonTaskRunner reminderSingletonTaskRunner,
            BirthdayRewardNotifierDomainService birthdayRewardNotifierDomainService,
            ReminderNotifierDomainService reminderNotifierDomainService)
        {
            this.birthdaySingletonTaskRunner = birthdaySingletonTaskRunner;
            this.reminderSingletonTaskRunner = reminderSingletonTaskRunner;
            this.birthdayRewardNotifierDomainService = birthdayRewardNotifierDomainService;
            this.reminderNotifierDomainService = reminderNotifierDomainService;
        }

        public Task AllShardsReadyAsync()
        {
            birthdaySingletonTaskRunner.StartTaskIfNotStarted(async () =>
            {
                await birthdayRewardNotifierDomainService.StartBirthdayRewardCheckerAsync();
            });

            reminderSingletonTaskRunner.StartTaskIfNotStarted(async () =>
            {
                await reminderNotifierDomainService.StartReminderCheckerAsync();
            });

            return Task.CompletedTask;
        }
    }
}
