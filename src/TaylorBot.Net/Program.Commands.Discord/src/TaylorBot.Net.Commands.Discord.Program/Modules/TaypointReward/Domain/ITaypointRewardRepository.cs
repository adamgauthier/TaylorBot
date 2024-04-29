using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;

public record RewardedUserResult(SnowflakeId UserId, long NewTaypointCount);

public interface ITaypointRewardRepository
{
    ValueTask<IReadOnlyCollection<RewardedUserResult>> RewardUsersAsync(IReadOnlyCollection<DiscordUser> users, int taypointCount);
}
