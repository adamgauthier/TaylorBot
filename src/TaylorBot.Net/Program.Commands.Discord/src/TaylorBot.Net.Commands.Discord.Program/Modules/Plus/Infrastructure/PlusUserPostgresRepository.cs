using Dapper;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Infrastructure;

public class PlusUserPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IPlusUserRepository
{
    private sealed record PlusGuildDtoDto(bool active, int max_plus_guilds, string? guild_name);

    public async ValueTask<PlusUser?> GetPlusUserAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var plusGuilds = (await connection.QueryAsync<PlusGuildDtoDto>(
            """
            SELECT active, max_plus_guilds, guild_name
            FROM plus.plus_users
            LEFT JOIN plus.plus_guilds ON user_id = plus_user_id AND state = 'enabled'
            LEFT JOIN guilds.guilds ON plus.plus_guilds.guild_id = guilds.guilds.guild_id
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        )).ToList();

        return plusGuilds.Count > 0 ?
            new PlusUser(
                IsActive: plusGuilds[0].active,
                MaxPlusGuilds: plusGuilds[0].max_plus_guilds,
                ActivePlusGuilds: plusGuilds[0].guild_name != null ? plusGuilds.Select(g => g.guild_name!).ToList() : Array.Empty<string>()
            ) :
            null;
    }

    public async ValueTask AddPlusGuildAsync(DiscordUser user, CommandGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO plus.plus_guilds (guild_id, plus_user_id, state)
            VALUES (@GuildId, @UserId, 'enabled')
            ON CONFLICT (guild_id, plus_user_id)
            DO UPDATE SET state = excluded.state;
            """,
            new
            {
                GuildId = $"{guild.Id}",
                UserId = $"{user.Id}",
            }
        );
    }

    public async ValueTask DisablePlusGuildAsync(DiscordUser user, CommandGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE plus.plus_guilds
            SET state = 'user_disabled'
            WHERE guild_id = @GuildId AND plus_user_id = @UserId;
            """,
            new
            {
                GuildId = $"{guild.Id}",
                UserId = $"{user.Id}",
            }
        );
    }
}
