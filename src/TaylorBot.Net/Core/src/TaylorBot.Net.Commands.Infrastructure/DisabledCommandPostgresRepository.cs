using Dapper;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class DisabledCommandPostgresRepository : IDisabledCommandRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public DisabledCommandPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask<string> InsertOrGetCommandDisabledMessageAsync(CommandInfo command)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            return await connection.QuerySingleAsync<string>(
                @"INSERT INTO commands.commands (name, aliases, module_name) VALUES (@CommandName, @Aliases, @ModuleName)
                ON CONFLICT (name) DO UPDATE SET
                    aliases = excluded.aliases,
                    module_name = excluded.module_name
                RETURNING disabled_message;",
                new
                {
                    CommandName = command.Aliases.First(),
                    Aliases = command.Aliases,
                    ModuleName = command.Module.Name
                }
            );
        }

        public async ValueTask EnableGloballyAsync(string commandName)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE commands.commands SET disabled_message = '' WHERE name = @CommandName;",
                new
                {
                    CommandName = commandName
                }
            );
        }

        public async ValueTask<string> DisableGloballyAsync(string commandName, string disabledMessage)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            return await connection.QuerySingleAsync<string>(
                @"UPDATE commands.commands SET disabled_message = @DisabledMessage WHERE name = @CommandName
                RETURNING disabled_message;",
                new
                {
                    CommandName = commandName,
                    DisabledMessage = disabledMessage
                }
            );
        }
    }
}
