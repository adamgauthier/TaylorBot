using Dapper;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.UsernameTracker.Domain;
using TaylorBot.Net.Core.Infrastructure;
using Discord;

namespace TaylorBot.Net.UsernameTracker.Infrastructure
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
    }
}
