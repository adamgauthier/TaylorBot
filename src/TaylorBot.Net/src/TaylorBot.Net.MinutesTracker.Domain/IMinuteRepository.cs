using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.MinutesTracker.Domain
{
    public interface IMinuteRepository
    {
        Task AddMinutesToActiveMembersAsync(long minutesToAdd, TimeSpan minimumTimeSpanSinceLastSpoke, long minutesRequiredForReward, long pointsReward);
    }
}
