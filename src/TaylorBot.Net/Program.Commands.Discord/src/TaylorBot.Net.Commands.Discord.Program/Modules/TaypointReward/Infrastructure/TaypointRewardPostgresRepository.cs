using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Taypoints;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Infrastructure;

public class TaypointRewardPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : ITaypointRewardRepository
{
    public async ValueTask<IReadOnlyCollection<RewardedUserResult>> RewardUsersAsync(IReadOnlyCollection<DiscordUser> users, int taypointCount)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var results = await TaypointPostgresUtil.AddTaypointsForMultipleUsersAsync(connection, taypointCount, users.Select(u => u.Id).ToList());

        return results.Select(u => new RewardedUserResult(
            UserId: new SnowflakeId(u.user_id),
            NewTaypointCount: u.taypoint_count
        )).ToList();
    }
}
