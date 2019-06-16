using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.BirthdayReward.Domain
{
    public interface IBirthdayRepository
    {
        Task<IEnumerable<RewardedUser>> RewardEligibleUsersAsync(long rewardAmount);
    }
}
