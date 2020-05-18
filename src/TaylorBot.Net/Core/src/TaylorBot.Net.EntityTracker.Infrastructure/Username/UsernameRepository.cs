using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.EntityTracker.Domain.Username;

namespace TaylorBot.Net.EntityTracker.Infrastructure.Username
{
    public class UsernameRepository : IUsernameRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public UsernameRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask AddNewUsernameAsync(IUser user)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

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
}
