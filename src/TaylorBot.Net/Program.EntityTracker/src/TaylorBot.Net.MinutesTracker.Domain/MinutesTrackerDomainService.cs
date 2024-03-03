using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.MinutesTracker.Domain.Options;

namespace TaylorBot.Net.MinutesTracker.Domain;

public interface IMinuteRepository
{
    Task AddMinuteToActiveUsersAsync(TimeSpan minimumTimeSpanSinceLastSpoke);
    Task AddMinuteAndPointToActiveUsersAsync(TimeSpan minimumTimeSpanSinceLastSpoke);
}

public class MinutesTrackerDomainService(
    ILogger<MinutesTrackerDomainService> logger,
    IOptionsMonitor<MinutesTrackerOptions> optionsMonitor,
    IMinuteRepository minuteRepository)
{
    public async Task StartMinutesAdderAsync()
    {
        var minuteCount = 1;

        while (true)
        {
            var options = optionsMonitor.CurrentValue;

            try
            {
                // Every 6 minutes, also give a point
                if (minuteCount % 6 == 0)
                {
                    await minuteRepository.AddMinuteAndPointToActiveUsersAsync(options.MinimumTimeSpanSinceLastSpoke);
                    minuteCount = 0;
                    logger.LogDebug("Added a minute and point to active users");
                }
                else
                {
                    await minuteRepository.AddMinuteToActiveUsersAsync(options.MinimumTimeSpanSinceLastSpoke);
                    logger.LogDebug("Added a minute to active users");
                }

                minuteCount++;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Exception occurred when attempting to add minutes to active members.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }
}
