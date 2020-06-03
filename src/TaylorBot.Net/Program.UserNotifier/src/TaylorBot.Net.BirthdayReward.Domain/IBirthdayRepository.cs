using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.BirthdayReward.Domain
{
    public interface IBirthdayRepository
    {
        ValueTask<IReadOnlyCollection<RewardedUser>> RewardEligibleUsersAsync(long rewardAmount);
    }
}
