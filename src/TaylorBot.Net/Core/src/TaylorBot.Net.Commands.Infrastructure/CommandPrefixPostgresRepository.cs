using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class CommandPrefixPostgresRepository : ICommandPrefixRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public CommandPrefixPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask<string> GetOrInsertGuildPrefixAsync(IGuild guild)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            return await connection.QuerySingleAsync<string>(
                """
                INSERT INTO guilds.guilds (guild_id, guild_name, previous_guild_name) VALUES (@GuildId, @GuildName, NULL)
                ON CONFLICT (guild_id) DO UPDATE SET
                    previous_guild_name = guilds.guilds.guild_name,
                    guild_name = excluded.guild_name
                RETURNING prefix;
                """,
                new
                {
                    GuildId = guild.Id.ToString(),
                    GuildName = guild.Name
                }
            );
        }

        public async ValueTask ChangeGuildPrefixAsync(IGuild guild, string prefix)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "UPDATE guilds.guilds SET prefix = @Prefix WHERE guild_id = @GuildId;",
                new
                {
                    Prefix = prefix,
                    GuildId = guild.Id.ToString()
                }
            );
        }
    }
}
