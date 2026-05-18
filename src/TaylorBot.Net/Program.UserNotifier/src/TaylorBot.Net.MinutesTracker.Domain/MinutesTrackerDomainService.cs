using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.MinutesTracker.Domain.Options;

namespace TaylorBot.Net.MinutesTracker.Domain;

public interface IMinuteRepository
{
    Task AddMinuteToActiveUsersAsync(TimeSpan minimumTimeSpanSinceLastSpoke);
    Task AddMinuteAndPointToActiveUsersAsync(TimeSpan minimumTimeSpanSinceLastSpoke);
}

public partial class MinutesTrackerDomainService(
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
                    LogAddedMinuteAndPointToActiveUsers();
                }
                else
                {
                    await minuteRepository.AddMinuteToActiveUsersAsync(options.MinimumTimeSpanSinceLastSpoke);
                    LogAddedMinuteToActiveUsers();
                }

                minuteCount++;
            }
            catch (Exception exception)
            {
                LogExceptionAddingMinutes(exception);
            }

            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Added a minute and point to active users")]
    private partial void LogAddedMinuteAndPointToActiveUsers();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Added a minute to active users")]
    private partial void LogAddedMinuteToActiveUsers();

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred when attempting to add minutes to active members.")]
    private partial void LogExceptionAddingMinutes(Exception exception);
}
