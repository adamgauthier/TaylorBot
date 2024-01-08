using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.MinutesTracker.Domain.Options;

namespace TaylorBot.Net.MinutesTracker.Domain;

public class MinutesTrackerDomainService(
    ILogger<MinutesTrackerDomainService> logger,
    IOptionsMonitor<MinutesTrackerOptions> optionsMonitor,
    IMinuteRepository minuteRepository)
{
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
                logger.LogError(exception, "Exception occurred when attempting to add minutes to active members.");
            }

            await Task.Delay(options.TimeSpanBetweenMinutesAdding);
        }
    }
}
