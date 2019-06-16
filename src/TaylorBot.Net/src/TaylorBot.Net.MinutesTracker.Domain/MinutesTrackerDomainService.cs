using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.MinutesTracker.Domain.Options;
using System;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.MinutesTracker.Domain
{
    public class MinutesTrackerDomainService
    {
        private readonly ILogger<MinutesTrackerDomainService> logger;
        private readonly IOptionsMonitor<MinutesTrackerOptions> optionsMonitor;
        private readonly IMinuteRepository minuteRepository;

        public MinutesTrackerDomainService(
            ILogger<MinutesTrackerDomainService> logger,
            IOptionsMonitor<MinutesTrackerOptions> optionsMonitor,
            IMinuteRepository minuteRepository)
        {
            this.logger = logger;
            this.optionsMonitor = optionsMonitor;
            this.minuteRepository = minuteRepository;
        }

        public async Task StartMinutesAdderAsync()
        {
            while (true)
            {
                var options = optionsMonitor.CurrentValue;

                try
                {
                    await minuteRepository.AddMinutesToActiveMembersAsync(
                        minutesToAdd: options.MinutesToAdd,
                        minimumTimeSpanSinceLastSpoke: options.MinimumTimeSpanSinceLastSpoke,
                        minutesRequiredForReward: options.MinutesRequiredForReward,
                        pointsReward: options.PointsReward
                    );
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, LogString.From("Exception occurred when attempting to add minutes to active members."));
                }

                await Task.Delay(options.TimeSpanBetweenMinutesAdding);
            }
        }
    }
}
