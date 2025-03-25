using Dapper;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure;

public class CommandPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : ICommandRepository
{
    private sealed record CommandDto(string name, string module_name);

    public async ValueTask<IReadOnlyCollection<ICommandRepository.Command>> GetAllCommandsAsync()
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var commandDtos = await connection.QueryAsync<CommandDto>("SELECT name, module_name FROM commands.commands;");

        return [.. commandDtos.Select(dto => new ICommandRepository.Command(
            Name: dto.name,
            ModuleName: dto.module_name
        ))];
    }

    public async ValueTask<ICommandRepository.Command?> FindCommandByAliasAsync(string commandAlias)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var command = await connection.QuerySingleOrDefaultAsync<CommandDto>(
            "SELECT name, module_name FROM commands.commands WHERE name = @NameOrAlias OR @NameOrAlias = ANY(aliases);",
            new
            {
                NameOrAlias = commandAlias,
            }
        );

        return command != null ? new ICommandRepository.Command(command.name, command.module_name) : null;
    }
}
