using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.BirthdayReward.Domain;

public interface IBirthdayCalendarRepository
{
    Task RefreshBirthdayCalendarAsync();
}

public partial class BirthdayCalendarDomainService(ILogger<BirthdayCalendarDomainService> logger, IBirthdayCalendarRepository birthdayCalendarRepository)
{
    public async Task StartRefreshingBirthdayCalendarAsync()
    {
        while (true)
        {
            try
            {
                LogRefreshingBirthdayCalendar();
                await birthdayCalendarRepository.RefreshBirthdayCalendarAsync();
            }
            catch (Exception e)
            {
                LogUnhandledExceptionRefreshingCalendar(e);
                await Task.Delay(TimeSpan.FromSeconds(30));
                continue;
            }

            await Task.Delay(TimeSpan.FromHours(12));
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Refreshing birthday calendar")]
    private partial void LogRefreshingBirthdayCalendar();

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in RefreshBirthdayCalendarAsync.")]
    private partial void LogUnhandledExceptionRefreshingCalendar(Exception exception);
}
