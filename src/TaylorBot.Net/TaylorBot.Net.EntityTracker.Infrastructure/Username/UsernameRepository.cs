using Dapper;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.Core.Infrastructure;
using Discord;
using TaylorBot.Net.EntityTracker.Domain.Username;

namespace TaylorBot.Net.EntityTracker.Infrastructure.Username
{
    public class UsernameRepository : PostgresRepository, IUsernameRepository
    {
        public UsernameRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task AddNewUsernameAsync(IUser user)
        {
            using (var connection = Connection)
            {
                connection.Open();

                await connection.ExecuteAsync(
                    "INSERT INTO users.usernames (user_id, username) VALUES (@UserId, @Username);",
                    new
                    {
                        UserId = user.Id.ToString(),
                        Username = user.Username
                    }
                );
            }
        }

        public async Task<string> GetLatestUsernameAsync(IUser user)
        {
            using (var connection = Connection)
            {
                connection.Open();

                var latestUsernameDto = await connection.QuerySingleOrDefaultAsync<LatestUsernameDto>(
                    @"SELECT u.username
                    FROM (
                        SELECT user_id, MAX(changed_at) AS max_changed_at
                        FROM users.usernames
                        GROUP BY user_id
                    ) AS maxed
                    JOIN users.usernames AS u ON u.user_id = maxed.user_id AND u.changed_at = maxed.max_changed_at
                    WHERE u.user_id = @UserId;",
                    new
                    {
                        UserId = user.Id.ToString()
                    }
                );

                return latestUsernameDto?.username;
            }
        }
    }
}
