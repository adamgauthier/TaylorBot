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
    private int _minuteCount = 1;

    public async Task<TimeSpan> RunMinutesAdderCycleAsync()
    {
        var options = optionsMonitor.CurrentValue;

        try
        {
            // Every 6 minutes, also give a point
            if (_minuteCount % 6 == 0)
            {
                await minuteRepository.AddMinuteAndPointToActiveUsersAsync(options.MinimumTimeSpanSinceLastSpoke);
                _minuteCount = 0;
                LogAddedMinuteAndPointToActiveUsers();
            }
            else
            {
                await minuteRepository.AddMinuteToActiveUsersAsync(options.MinimumTimeSpanSinceLastSpoke);
                LogAddedMinuteToActiveUsers();
            }

            _minuteCount++;
        }
        catch (Exception exception)
        {
            LogExceptionAddingMinutes(exception);
        }

        return TimeSpan.FromMinutes(1);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Added a minute and point to active users")]
    private partial void LogAddedMinuteAndPointToActiveUsers();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Added a minute to active users")]
    private partial void LogAddedMinuteToActiveUsers();

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred when attempting to add minutes to active members.")]
    private partial void LogExceptionAddingMinutes(Exception exception);
}
