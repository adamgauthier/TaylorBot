using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Infrastructure;

public class GenderPostgresRepository : IGenderRepository
{
    private readonly PostgresConnectionFactory _postgresConnectionFactory;

    public GenderPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
    {
        _postgresConnectionFactory = postgresConnectionFactory;
    }

    public async ValueTask<string?> GetGenderAsync(IUser user)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<string>(
            """
            SELECT attribute_value FROM attributes.text_attributes
            WHERE user_id = @UserId AND attribute_id = 'gender';
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }

    public async ValueTask SetGenderAsync(IUser user, string gender)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO attributes.text_attributes (attribute_id, user_id, attribute_value)
            VALUES ('gender', @UserId, @AttributeValue)
            ON CONFLICT (attribute_id, user_id) DO UPDATE
              SET attribute_value = excluded.attribute_value;
            """,
            new
            {
                UserId = $"{user.Id}",
                AttributeValue = gender,
            }
        );
    }

    public async ValueTask ClearGenderAsync(IUser user)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "DELETE FROM attributes.text_attributes WHERE user_id = @UserId AND attribute_id = 'gender';",
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }
}
