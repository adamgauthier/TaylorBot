using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Infrastructure
{
    public class TaypointRewardPostgresRepository : ITaypointRewardRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public TaypointRewardPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        private class RewardedUserDto
        {
            public string user_id { get; set; } = null!;
            public long taypoint_count { get; set; }
        }

        public async ValueTask<IReadOnlyCollection<RewardedUserResult>> RewardUsersAsync(IReadOnlyCollection<IUser> users, int taypointCount)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            var results = await connection.QueryAsync<RewardedUserDto>(
                @"UPDATE users.users
                SET taypoint_count = taypoint_count + @PointsToAdd
                WHERE user_id = ANY(@UserIds)
                RETURNING user_id, taypoint_count;",
                new
                {
                    PointsToAdd = taypointCount,
                    UserIds = users.Select(u => u.Id.ToString()).ToList()
                }
            );

            return results.Select(u => new RewardedUserResult(
                UserId: new SnowflakeId(u.user_id),
                NewTaypointCount: u.taypoint_count
            )).ToList();
        }
    }
}
