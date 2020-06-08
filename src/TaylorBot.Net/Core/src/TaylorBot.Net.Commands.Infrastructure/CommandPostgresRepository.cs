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
            public string name { get; set; }
            public string module_name { get; set; }
        }

        public async ValueTask<IReadOnlyCollection<ICommandRepository.Command>> GetAllCommandsAsync()
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var commandUsageDtos = await connection.QueryAsync<CommandDto>("SELECT name, module_name FROM commands.commands;");

            return commandUsageDtos.Select(dto => new ICommandRepository.Command(
                name: dto.name,
                moduleName: dto.module_name
            )).ToList();
        }
    }
}
