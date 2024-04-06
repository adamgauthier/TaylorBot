using Dapper;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Infrastructure;

public class PlusPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IPlusRepository
{
    public async ValueTask<bool> IsActivePlusUserAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<bool>(
            """
            SELECT EXISTS(
                SELECT FROM plus.plus_users WHERE user_id = @UserId AND active = TRUE
            );
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }

    public async ValueTask<bool> IsActivePlusGuildAsync(CommandGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<bool>(
            """
            SELECT EXISTS(
                SELECT FROM plus.plus_guilds WHERE guild_id = @GuildId AND state = 'enabled'
            );
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );
    }
}
