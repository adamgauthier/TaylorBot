using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Infrastructure;

public class BirthdayRoleConfigPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IBirthdayRoleConfigRepository
{
    public async Task<string?> GetRoleForGuildAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<string?>(
            """
            SELECT role_id
            FROM plus.birthday_roles
            WHERE guild_id = @GuildId;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );
    }

    public async Task AddRoleForGuildAsync(IGuild guild, IRole role)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO plus.birthday_roles (guild_id, role_id)
            VALUES (@GuildId, @RoleId)
            ON CONFLICT (guild_id) DO UPDATE SET
                role_id = excluded.role_id;
            """,
            new
            {
                GuildId = $"{guild.Id}",
                RoleId = $"{role.Id}",
            }
        );
    }

    public async Task RemoveRoleForGuildAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            """
            DELETE FROM plus.birthday_roles
            WHERE guild_id = @GuildId;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        await connection.ExecuteAsync(
            """
            DELETE FROM plus.birthday_roles_given
            WHERE guild_id = @GuildId;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        transaction.Commit();
    }
}
