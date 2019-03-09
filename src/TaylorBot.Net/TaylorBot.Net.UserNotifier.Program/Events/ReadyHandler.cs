using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.Reminder.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events
{
    public class ReadyHandler : IAllReadyHandler
    {
        private readonly ILogger<ReadyHandler> logger;
        private readonly TaylorBotClient taylorBotClient;
        private readonly SingletonTaskRunner birthdaySingletonTaskRunner;
        private readonly SingletonTaskRunner reminderSingletonTaskRunner;
        private readonly BirthdayRewardNotifierDomainService birthdayRewardNotifierDomainService;
        private readonly ReminderNotifierDomainService reminderNotifierDomainService;

        public ReadyHandler(
            ILogger<ReadyHandler> logger,
            TaylorBotClient taylorBotClient,
            SingletonTaskRunner birthdaySingletonTaskRunner,
            SingletonTaskRunner reminderSingletonTaskRunner,
            BirthdayRewardNotifierDomainService birthdayRewardNotifierDomainService,
            ReminderNotifierDomainService reminderNotifierDomainService)
        {
            this.logger = logger;
            this.taylorBotClient = taylorBotClient;
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
