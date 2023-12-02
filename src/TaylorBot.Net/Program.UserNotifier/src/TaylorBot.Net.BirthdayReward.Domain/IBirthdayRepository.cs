namespace TaylorBot.Net.BirthdayReward.Domain;

public interface IBirthdayRepository
{
    ValueTask<IReadOnlyCollection<RewardedUser>> RewardEligibleUsersAsync(long rewardAmount);
}
