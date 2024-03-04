using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.BirthdayReward.Domain;

public interface IBirthdayCalendarRepository
{
    Task RefreshBirthdayCalendarAsync();
}

public class BirthdayCalendarDomainService(ILogger<BirthdayCalendarDomainService> logger, IBirthdayCalendarRepository birthdayCalendarRepository)
{
    public async Task StartRefreshingBirthdayCalendarAsync()
    {
        while (true)
        {
            try
            {
                logger.LogInformation("Refreshing birthday calendar");
                await birthdayCalendarRepository.RefreshBirthdayCalendarAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unhandled exception in {nameof(birthdayCalendarRepository.RefreshBirthdayCalendarAsync)}.");
                await Task.Delay(TimeSpan.FromSeconds(30));
                continue;
            }

            await Task.Delay(TimeSpan.FromHours(12));
        }
    }
}
