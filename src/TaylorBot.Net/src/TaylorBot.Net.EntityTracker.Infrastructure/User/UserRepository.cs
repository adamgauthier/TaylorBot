using Dapper;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.Core.Infrastructure;
using Discord;
using TaylorBot.Net.EntityTracker.Domain.User;
using System.Linq;

namespace TaylorBot.Net.EntityTracker.Infrastructure.User
{
    public class UserRepository : PostgresRepository, IUserRepository
    {
        public UserRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task<UserAddedResult> AddNewUserAsync(IUser user)
        {
            using (var connection = Connection)
            {
                connection.Open();

                var results = await connection.QueryAsync(
                    @"INSERT INTO users.users (user_id, is_bot) VALUES (@UserId, @IsBot)
                    ON CONFLICT (user_id) DO NOTHING RETURNING user_id;",
                    new
                    {
                        UserId = user.Id.ToString(),
                        IsBot = user.IsBot
                    }
                );

                return new UserAddedResult(wasAdded: results.Any());
            }
        }
    }
}
