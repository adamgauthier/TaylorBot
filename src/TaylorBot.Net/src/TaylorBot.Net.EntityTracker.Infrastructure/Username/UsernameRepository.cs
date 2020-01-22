using Dapper;
using Discord;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;
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
            using var connection = Connection;
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
}
