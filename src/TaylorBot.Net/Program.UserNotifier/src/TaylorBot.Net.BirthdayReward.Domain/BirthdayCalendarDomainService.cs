using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.BirthdayReward.Domain;

public interface IBirthdayCalendarRepository
{
    Task RefreshBirthdayCalendarAsync();
}

public partial class BirthdayCalendarDomainService(ILogger<BirthdayCalendarDomainService> logger, IBirthdayCalendarRepository birthdayCalendarRepository)
{
    public async Task<TimeSpan> RunRefreshingBirthdayCalendarCycleAsync()
    {
        try
        {
            LogRefreshingBirthdayCalendar();
            await birthdayCalendarRepository.RefreshBirthdayCalendarAsync();
        }
        catch (Exception e)
        {
            LogUnhandledExceptionRefreshingCalendar(e);
            return TimeSpan.FromSeconds(30);
        }

        return TimeSpan.FromHours(12);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Refreshing birthday calendar")]
    private partial void LogRefreshingBirthdayCalendar();

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in RefreshBirthdayCalendarAsync.")]
    private partial void LogUnhandledExceptionRefreshingCalendar(Exception exception);
}
