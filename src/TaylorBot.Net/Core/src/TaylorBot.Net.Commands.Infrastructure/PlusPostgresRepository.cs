using Dapper;
using Discord;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure;

public class PlusPostgresRepository : IPlusRepository
{
    private readonly PostgresConnectionFactory _postgresConnectionFactory;

    public PlusPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
    {
        _postgresConnectionFactory = postgresConnectionFactory;
    }

    public async ValueTask<bool> IsActivePlusUserAsync(IUser user)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<bool>(
            @"SELECT EXISTS(
                    SELECT FROM plus.plus_users WHERE user_id = @UserId AND active = TRUE
                );",
            new
            {
                UserId = user.Id.ToString()
            }
        );
    }

    public async ValueTask<bool> IsActivePlusGuildAsync(IGuild guild)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<bool>(
            @"SELECT EXISTS(
                    SELECT FROM plus.plus_guilds WHERE guild_id = @GuildId AND state = 'enabled'
                );",
            new
            {
                GuildId = guild.Id.ToString()
            }
        );
    }
}
