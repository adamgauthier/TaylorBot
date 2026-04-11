using Dapper;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;

public record TextAttributeValue(string Value, DateTimeOffset SetAt);

public class TextAttributePostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
{
    private sealed record TextAttributeDto(string attribute_value, DateTime set_at);

    public async ValueTask<TextAttributeValue?> GetAttributeAsync(DiscordUser user, string attributeId)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var dto = await connection.QuerySingleOrDefaultAsync<TextAttributeDto?>(
            """
            SELECT attribute_value, set_at FROM attributes.text_attributes
            WHERE user_id = @UserId AND attribute_id = @AttributeId;
            """,
            new
            {
                UserId = $"{user.Id}",
                AttributeId = attributeId,
            }
        );

        return dto != null ? new(dto.attribute_value, new DateTimeOffset(dto.set_at, TimeSpan.Zero)) : null;
    }

    public async ValueTask SetAttributeAsync(DiscordUser user, string attributeId, string attributeValue)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO attributes.text_attributes (attribute_id, user_id, attribute_value)
            VALUES (@AttributeId, @UserId, @AttributeValue)
            ON CONFLICT (attribute_id, user_id) DO UPDATE
              SET attribute_value = excluded.attribute_value,
                  set_at = NOW();
            """,
            new
            {
                UserId = $"{user.Id}",
                AttributeId = attributeId,
                AttributeValue = attributeValue,
            }
        );
    }

    public async ValueTask ClearAttributeAsync(DiscordUser user, string attributeId)
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
