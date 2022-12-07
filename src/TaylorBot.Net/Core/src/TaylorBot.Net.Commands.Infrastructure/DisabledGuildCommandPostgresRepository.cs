using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class DisabledGuildCommandPostgresRepository : IDisabledGuildCommandRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public DisabledGuildCommandPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask DisableInAsync(IGuild guild, string commandName)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT INTO guilds.guild_commands (guild_id, command_name, disabled)
                VALUES (@GuildId, @CommandName, TRUE)
                ON CONFLICT (guild_id, command_name) DO UPDATE
                    SET disabled = excluded.disabled;",
                new
                {
                    GuildId = guild.Id.ToString(),
                    CommandName = commandName
                }
            );
        }

        public async ValueTask EnableInAsync(IGuild guild, string commandName)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE guilds.guild_commands SET disabled = FALSE WHERE guild_id = @GuildId;",
                new
                {
                    GuildId = guild.Id.ToString(),
                    CommandName = commandName
                }
            );
        }

        public async ValueTask<bool> IsGuildCommandDisabledAsync(IGuild guild, CommandMetadata command)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            var disabled = await connection.QuerySingleOrDefaultAsync<bool>(
                """
                SELECT EXISTS(
                    SELECT 1 FROM guilds.guild_commands
                    WHERE guild_id = @GuildId AND command_name = @CommandName AND disabled = TRUE
                );
                """,
                new
                {
                    GuildId = guild.Id.ToString(),
                    CommandName = command.Name
                }
            );

            return disabled;
        }
    }
}
