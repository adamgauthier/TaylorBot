using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.BirthdayReward.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events
{
    public class ReadyHandler : IAllReadyHandler
    {
        private readonly ILogger<ReadyHandler> logger;
        private readonly TaylorBotClient taylorBotClient;
        private readonly SingletonTaskRunner singletonTaskRunner;
        private readonly BirthdayRewardNotifierDomainService birthdayRewardNotifierDomainService;

        public ReadyHandler(ILogger<ReadyHandler> logger, TaylorBotClient taylorBotClient, SingletonTaskRunner singletonTaskRunner, BirthdayRewardNotifierDomainService birthdayRewardNotifierDomainService)
        {
            this.logger = logger;
            this.taylorBotClient = taylorBotClient;
            this.singletonTaskRunner = singletonTaskRunner;
            this.birthdayRewardNotifierDomainService = birthdayRewardNotifierDomainService;
        }

        public Task AllShardsReadyAsync()
        {
            singletonTaskRunner.StartTaskIfNotStarted(async () =>
            {
                await birthdayRewardNotifierDomainService.StartBirthdayRewardCheckerAsync();
            });

            return Task.CompletedTask;
        }
    }
}
