using Dapper;
using Discord;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.EntityTracker.Domain.Username;

namespace TaylorBot.Net.EntityTracker.Infrastructure.Username;

public class UsernameRepository(PostgresConnectionFactory postgresConnectionFactory) : IUsernameRepository
{
    public async ValueTask AddNewUsernameAsync(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

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
