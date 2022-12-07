using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Knowledge.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Knowledge.Infrastructure
{
    public class ZodiacSignPostgresRepository : IZodiacSignRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public ZodiacSignPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask<string?> GetZodiacForUserAsync(IUser user)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<string?>(
                @"SELECT zodiac(birthday) FROM attributes.birthdays WHERE user_id = @UserId;",
                new
                {
                    UserId = user.Id.ToString(),
                }
            );
        }
    }
}
