using Dapper;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure;

public class CommandPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : ICommandRepository
{
    private sealed record CommandDto(string name);

    public async ValueTask<ICommandRepository.Command?> FindCommandByAliasAsync(string commandAlias)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var command = await connection.QuerySingleOrDefaultAsync<CommandDto>(
            "SELECT name FROM commands.commands WHERE name = @NameOrAlias OR @NameOrAlias = ANY(aliases);",
            new
            {
                NameOrAlias = commandAlias,
            }
        );

        return command != null ? new ICommandRepository.Command(command.name) : null;
    }
}
