using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.TaypointReward.Domain
{
    public class RewardedUserResult
    {
        public SnowflakeId UserId { get; }
        public long NewTaypointCount { get; }

        public RewardedUserResult(SnowflakeId userId, long newTaypointCount)
        {
            UserId = userId;
            NewTaypointCount = newTaypointCount;
        }
    }

    public interface ITaypointRewardRepository
    {
        ValueTask<IReadOnlyCollection<RewardedUserResult>> RewardUsersAsync(IReadOnlyCollection<IUser> users, int taypointCount);
    }
}
