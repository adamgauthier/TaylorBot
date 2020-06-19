using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Jail.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Jail.Infrastructure
{
    public class JailPostgresRepository : IJailRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public JailPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask SetJailRoleAsync(IGuild guild, IRole jailRole)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

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
    }
}
