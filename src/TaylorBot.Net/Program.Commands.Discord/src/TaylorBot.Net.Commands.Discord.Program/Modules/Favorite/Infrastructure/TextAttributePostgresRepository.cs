using Dapper;
using Discord;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;

public class TextAttributePostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
{
    public async ValueTask<string?> GetAttributeAsync(IUser user, string attributeId)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<string>(
            """
            SELECT attribute_value FROM attributes.text_attributes
            WHERE user_id = @UserId AND attribute_id = @AttributeId;
            """,
            new
            {
                UserId = $"{user.Id}",
                AttributeId = attributeId,
            }
        );
    }

    public async ValueTask SetAttributeAsync(IUser user, string attributeId, string attributeValue)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO attributes.text_attributes (attribute_id, user_id, attribute_value)
            VALUES (@AttributeId, @UserId, @AttributeValue)
            ON CONFLICT (attribute_id, user_id) DO UPDATE
              SET attribute_value = excluded.attribute_value;
            """,
            new
            {
                UserId = $"{user.Id}",
                AttributeId = attributeId,
                AttributeValue = attributeValue,
            }
        );
    }

    public async ValueTask ClearAttributeAsync(IUser user, string attributeId)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "DELETE FROM attributes.text_attributes WHERE user_id = @UserId AND attribute_id = @AttributeId;",
            new
            {
                UserId = $"{user.Id}",
                AttributeId = attributeId,
            }
        );
    }
}
