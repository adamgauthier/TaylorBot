using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.TaypointReward.Domain
{
    public record RewardedUserResult(SnowflakeId UserId, long NewTaypointCount);

    public interface ITaypointRewardRepository
    {
        ValueTask<IReadOnlyCollection<RewardedUserResult>> RewardUsersAsync(IReadOnlyCollection<IUser> users, int taypointCount);
    }
}
