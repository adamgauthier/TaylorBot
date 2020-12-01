using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class CommandPostgresRepository : ICommandRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public CommandPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        private class CommandDto
        {
            public string name { get; set; } = null!;
            public string module_name { get; set; } = null!;
        }

        public async ValueTask<IReadOnlyCollection<ICommandRepository.Command>> GetAllCommandsAsync()
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var commandDtos = await connection.QueryAsync<CommandDto>("SELECT name, module_name FROM commands.commands;");

            return commandDtos.Select(dto => new ICommandRepository.Command(
                name: dto.name,
                moduleName: dto.module_name
            )).ToList();
        }

        public async ValueTask<ICommandRepository.Command?> FindCommandByAliasAsync(string alias)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var command = await connection.QuerySingleOrDefaultAsync<CommandDto>(
                "SELECT name, module_name FROM commands.commands WHERE name = @NameOrAlias OR @NameOrAlias = ANY(aliases);",
                new
                {
                    NameOrAlias = alias
                }
            );

            return command != null ? new ICommandRepository.Command(command.name, command.module_name) : null;
        }
    }
}
