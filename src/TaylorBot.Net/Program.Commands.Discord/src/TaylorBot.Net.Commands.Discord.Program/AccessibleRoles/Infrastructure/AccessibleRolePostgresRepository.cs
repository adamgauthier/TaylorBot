using Dapper;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.AccessibleRoles.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.AccessibleRoles.Infrastructure
{
    public class AccessibleRolePostgresRepository : IAccessibleRoleRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public AccessibleRolePostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask<bool> IsRoleAccessibleAsync(IRole role)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            return await connection.QuerySingleAsync<bool>(
                @"SELECT EXISTS(
                    SELECT role_id FROM guilds.guild_special_roles
                    WHERE guild_id = @GuildId AND role_id = @RoleId AND accessible = TRUE
                );",
                new
                {
                    GuildId = role.Guild.Id.ToString(),
                    RoleId = role.Id.ToString()
                }
            );
        }

        private class AccessibleRoleDto
        {
            public string role_id { get; set; } = null!;
        }

        public async ValueTask<IReadOnlyCollection<AccessibleRole>> GetAccessibleRolesAsync(IGuild guild)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var roles = await connection.QueryAsync<AccessibleRoleDto>(
                @"SELECT role_id FROM guilds.guild_special_roles
                WHERE guild_id = @GuildId AND accessible = TRUE;",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );

            return roles.Select(r => new AccessibleRole(
                new SnowflakeId(r.role_id))
            ).ToList();
        }
    }
}
