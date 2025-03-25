using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Infrastructure;

public class JailPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IJailRepository
{
    public async ValueTask SetJailRoleAsync(IGuild guild, IRole jailRole)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"INSERT INTO guilds.jail_roles (guild_id, jail_role_id, set_at) VALUES (@GuildId, @JailRoleId, CURRENT_TIMESTAMP)
                ON CONFLICT (guild_id) DO UPDATE SET
                    jail_role_id = excluded.jail_role_id,
                    set_at = excluded.set_at
                ;",
            new
            {
                GuildId = guild.Id.ToString(),
                JailRoleId = jailRole.Id.ToString()
            }
        );
    }

    private sealed class GetJailRoleDto
    {
        public string jail_role_id { get; set; } = null!;
    }

    public async ValueTask<JailRole?> GetJailRoleAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var role = await connection.QuerySingleOrDefaultAsync<GetJailRoleDto>(
            @"SELECT jail_role_id FROM guilds.jail_roles WHERE guild_id = @GuildId;",
            new
            {
                GuildId = guild.Id.ToString()
            }
        );

        return role != null ? new JailRole(new SnowflakeId(role.jail_role_id)) : null;
    }
}
