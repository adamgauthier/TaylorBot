using Humanizer;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.BirthdayReward.Domain;

public class RewardedUser(SnowflakeId userId, long rewardedPoints)
{
    public SnowflakeId UserId { get; } = userId;
    public long PointsAfterReward { get; } = rewardedPoints;

    public override string ToString()
    {
        return $"Rewarded User ID {UserId}, now has {"point".ToQuantity(PointsAfterReward)}";
    }
}
