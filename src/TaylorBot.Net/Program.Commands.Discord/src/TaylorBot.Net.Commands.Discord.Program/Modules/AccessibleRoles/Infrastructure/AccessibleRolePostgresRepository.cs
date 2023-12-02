using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Infrastructure;

public class AccessibleRolePostgresRepository : IAccessibleRoleRepository
{
    private readonly PostgresConnectionFactory _postgresConnectionFactory;

    public AccessibleRolePostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
    {
        _postgresConnectionFactory = postgresConnectionFactory;
    }

    public async ValueTask<bool> IsRoleAccessibleAsync(IRole role)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<bool>(
            @"SELECT EXISTS(
                    SELECT role_id FROM guilds.guild_accessible_roles
                    WHERE guild_id = @GuildId AND role_id = @RoleId AND accessible = TRUE
                );",
            new
            {
                GuildId = role.Guild.Id.ToString(),
                RoleId = role.Id.ToString()
            }
        );
    }

    private class GetSingleAccessibleRoleDto
    {
        public string? group_name { get; set; }
    }

    private class OtherAccessibleRoleInSameGroupDto
    {
        public string role_id { get; set; } = null!;
    }

    public async ValueTask<AccessibleRoleWithGroup?> GetAccessibleRoleAsync(IRole role)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var accessibleRole = await connection.QuerySingleOrDefaultAsync<GetSingleAccessibleRoleDto?>(
            @"SELECT group_name FROM guilds.guild_accessible_roles
                WHERE guild_id = @GuildId AND role_id = @RoleId AND accessible = TRUE;",
            new
            {
                GuildId = role.Guild.Id.ToString(),
                RoleId = role.Id.ToString()
            }
        );

        if (accessibleRole != null)
        {
            if (accessibleRole.group_name != null)
            {
                var otherRoles = await connection.QueryAsync<OtherAccessibleRoleInSameGroupDto>(
                    @"SELECT role_id FROM guilds.guild_accessible_roles
                        WHERE guild_id = @GuildId AND accessible = TRUE AND role_id != @RoleId AND group_name = @GroupName;",
                    new
                    {
                        GuildId = role.Guild.Id.ToString(),
                        RoleId = role.Id.ToString(),
                        GroupName = accessibleRole.group_name
                    }
                );
                return new AccessibleRoleWithGroup(
                    Group: new AccessibleRoleGroup(accessibleRole.group_name, otherRoles.Select(r => new SnowflakeId(r.role_id)).ToList())
                );
            }
            else
            {
                return new AccessibleRoleWithGroup(
                    Group: null
                );
            }
        }
        else
        {
            return null;
        }
    }

    private class AccessibleRoleDto
    {
        public string role_id { get; set; } = null!;
        public string? group_name { get; set; }
    }

    public async ValueTask<IReadOnlyCollection<AccessibleRole>> GetAccessibleRolesAsync(IGuild guild)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var roles = await connection.QueryAsync<AccessibleRoleDto>(
            @"SELECT role_id, group_name FROM guilds.guild_accessible_roles
                WHERE guild_id = @GuildId AND accessible = TRUE;",
            new
            {
                GuildId = guild.Id.ToString()
            }
        );

        return roles.Select(r => new AccessibleRole(
            RoleId: new SnowflakeId(r.role_id),
            GroupName: r.group_name
        )).ToList();
    }

    public async ValueTask AddAccessibleRoleAsync(IRole role)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"INSERT INTO guilds.guild_accessible_roles (guild_id, role_id, accessible)
                VALUES (@GuildId, @RoleId, TRUE)
                ON CONFLICT (guild_id, role_id) DO UPDATE
                    SET accessible = excluded.accessible;",
            new
            {
                GuildId = role.Guild.Id.ToString(),
                RoleId = role.Id.ToString()
            }
        );
    }

    public async ValueTask AddOrUpdateAccessibleRoleWithGroupAsync(IRole role, AccessibleGroupName groupName)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"INSERT INTO guilds.guild_accessible_roles (guild_id, role_id, accessible, group_name)
                VALUES (@GuildId, @RoleId, TRUE, @GroupName)
                ON CONFLICT (guild_id, role_id) DO UPDATE
                    SET group_name = excluded.group_name;",
            new
            {
                GuildId = role.Guild.Id.ToString(),
                RoleId = role.Id.ToString(),
                GroupName = groupName.Name
            }
        );
    }

    public async ValueTask RemoveAccessibleRoleAsync(IRole role)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"UPDATE guilds.guild_accessible_roles
                SET accessible = FALSE
                WHERE guild_id = @GuildId AND role_id = @RoleId;",
            new
            {
                GuildId = role.Guild.Id.ToString(),
                RoleId = role.Id.ToString()
            }
        );
    }

    public async ValueTask ClearGroupFromAccessibleRoleAsync(IRole role)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"UPDATE guilds.guild_accessible_roles
                SET group_name = NULL
                WHERE guild_id = @GuildId AND role_id = @RoleId;",
            new
            {
                GuildId = role.Guild.Id.ToString(),
                RoleId = role.Id.ToString()
            }
        );
    }
}
