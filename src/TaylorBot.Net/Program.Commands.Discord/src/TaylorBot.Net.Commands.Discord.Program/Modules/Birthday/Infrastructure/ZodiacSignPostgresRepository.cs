using Dapper;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Infrastructure;

public class ZodiacSignPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IZodiacSignRepository
{
    public async ValueTask<string?> GetZodiacForUserAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<string?>(
            "SELECT zodiac(birthday) FROM attributes.birthdays WHERE user_id = @UserId AND birthday != '0001-01-01';",
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }
}
