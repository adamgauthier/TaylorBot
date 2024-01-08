using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Infrastructure;

public class ZodiacSignPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IZodiacSignRepository
{
    public async ValueTask<string?> GetZodiacForUserAsync(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<string?>(
            @"SELECT zodiac(birthday) FROM attributes.birthdays WHERE user_id = @UserId;",
            new
            {
                UserId = user.Id.ToString(),
            }
        );
    }
}
