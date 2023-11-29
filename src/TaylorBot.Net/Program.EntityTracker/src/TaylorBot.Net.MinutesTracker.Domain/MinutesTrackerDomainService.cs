using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.MinutesTracker.Domain.Options;

namespace TaylorBot.Net.MinutesTracker.Domain
{
    public class MinutesTrackerDomainService
    {
        private readonly ILogger<MinutesTrackerDomainService> _logger;
        private readonly IOptionsMonitor<MinutesTrackerOptions> _optionsMonitor;
        private readonly IMinuteRepository _minuteRepository;

        public MinutesTrackerDomainService(
            ILogger<MinutesTrackerDomainService> logger,
            IOptionsMonitor<MinutesTrackerOptions> optionsMonitor,
            IMinuteRepository minuteRepository)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _minuteRepository = minuteRepository;
        }

        public async Task StartMinutesAdderAsync()
        {
            while (true)
            {
                var options = _optionsMonitor.CurrentValue;

                try
                {
                    await _minuteRepository.AddMinutesToActiveMembersAsync(
                        minutesToAdd: options.MinutesToAdd,
                        minimumTimeSpanSinceLastSpoke: options.MinimumTimeSpanSinceLastSpoke,
                        minutesRequiredForReward: options.MinutesRequiredForReward,
                        pointsReward: options.PointsReward
                    );
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Exception occurred when attempting to add minutes to active members.");
                }

                await Task.Delay(options.TimeSpanBetweenMinutesAdding);
            }
        }
    }
}
