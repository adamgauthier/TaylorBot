using Dapper;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.User;
using TaylorBot.Net.EntityTracker.Domain.Username;

namespace TaylorBot.Net.EntityTracker.Infrastructure.Username;

public class UsernameRepository(PostgresConnectionFactory postgresConnectionFactory) : IUsernameRepository
{
    public async ValueTask AddNewUsernameAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "INSERT INTO users.usernames (user_id, username) VALUES (@UserId, @Username);",
            new
            {
                UserId = $"{user.Id}",
                Username = user.Username,
            }
        );
    }
}
