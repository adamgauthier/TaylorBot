using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Valentines.Domain;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Events
{
    public class AllReadyHandler : IAllReadyHandler
    {
        private readonly SingletonTaskRunner _valentineSingletonTaskRunner;
        private readonly ValentineGiveawayDomainService _valentineGiveawayDomainService;

        public AllReadyHandler(SingletonTaskRunner valentineSingletonTaskRunner, ValentineGiveawayDomainService valentineGiveawayDomainService)
        {
            _valentineSingletonTaskRunner = valentineSingletonTaskRunner;
            _valentineGiveawayDomainService = valentineGiveawayDomainService;
        }

        public Task AllShardsReadyAsync()
        {
            _ = _valentineSingletonTaskRunner.StartTaskIfNotStarted(
                _valentineGiveawayDomainService.StartGiveawayAsync,
                nameof(ValentineGiveawayDomainService)
            );

            return Task.CompletedTask;
        }
    }
}
