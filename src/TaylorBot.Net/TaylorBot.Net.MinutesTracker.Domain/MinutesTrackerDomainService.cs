using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.MinutesTracker.Domain.Options;
using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.MinutesTracker.Domain
{
    public class MinutesTrackerDomainService
    {
        private readonly IOptionsMonitor<MinutesTrackerOptions> optionsMonitor;
        private readonly IMinuteRepository minuteRepository;
        private readonly TaylorBotClient taylorBotClient;

        public MinutesTrackerDomainService(
            IOptionsMonitor<MinutesTrackerOptions> optionsMonitor,
            IMinuteRepository minuteRepository,
            TaylorBotClient taylorBotClient)
        {
            this.optionsMonitor = optionsMonitor;
            this.minuteRepository = minuteRepository;
            this.taylorBotClient = taylorBotClient;
        }

        public async Task StartMinutesAdderAsync()
        {
            while (true)
            {
                var options = optionsMonitor.CurrentValue;

                await minuteRepository.AddMinutesToActiveMembersAsync(
                    minutesToAdd: options.MinutesToAdd,
                    minimumTimeSpanSinceLastSpoke: options.MinimumTimeSpanSinceLastSpoke,
                    minutesRequiredForReward: options.MinutesRequiredForReward,
                    pointsReward: options.PointsReward
                );

                await Task.Delay(options.TimeSpanBetweenMinutesAdding);
            }
        }
    }
}
