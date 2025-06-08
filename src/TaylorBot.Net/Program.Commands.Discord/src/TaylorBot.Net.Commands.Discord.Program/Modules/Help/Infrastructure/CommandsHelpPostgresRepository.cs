using Dapper;
using TaylorBot.Net.Commands.Discord.Program.Modules.Help.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Help.Infrastructure;

public class CommandsHelpPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : ICommandsHelpRepository
{
    public async Task<string?> GetCommandsHelpAsync()
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<string>(
            """
            SELECT info_value FROM configuration.application_info WHERE info_key = 'commands.md';
            """);
    }
}
