using Humanizer;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.BirthdayReward.Domain
{
    public class RewardedUser
    {
        public SnowflakeId UserId { get; }
        public long PointsAfterReward { get; }

        public RewardedUser(SnowflakeId userId, long rewardedPoints)
        {
            UserId = userId;
            PointsAfterReward = rewardedPoints;
        }

        public override string ToString()
        {
            return $"Rewarded User ID {UserId}, now has {"point".ToQuantity(PointsAfterReward)}";
        }
    }
}
