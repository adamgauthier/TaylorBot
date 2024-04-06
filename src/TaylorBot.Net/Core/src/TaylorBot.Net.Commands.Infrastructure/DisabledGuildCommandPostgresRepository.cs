using Dapper;
using Discord;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure;

public class DisabledGuildCommandPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IDisabledGuildCommandRepository
{
    public async ValueTask DisableInAsync(IGuild guild, string commandName)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO guilds.guild_commands (guild_id, command_name, disabled)
            VALUES (@GuildId, @CommandName, TRUE)
            ON CONFLICT (guild_id, command_name) DO UPDATE
                SET disabled = excluded.disabled;
            """,
            new
            {
                GuildId = $"{guild.Id}",
                CommandName = commandName,
            }
        );
    }

    public async ValueTask EnableInAsync(IGuild guild, string commandName)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "UPDATE guilds.guild_commands SET disabled = FALSE WHERE guild_id = @GuildId;",
            new
            {
                GuildId = $"{guild.Id}",
                CommandName = commandName,
            }
        );
    }

    public async ValueTask<GuildCommandDisabled> IsGuildCommandDisabledAsync(CommandGuild guild, CommandMetadata command)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var disabled = await connection.QuerySingleOrDefaultAsync<bool>(
            """
            SELECT EXISTS(
                SELECT 1 FROM guilds.guild_commands
                WHERE guild_id = @GuildId AND command_name = @CommandName AND disabled = TRUE
            );
            """,
            new
            {
                GuildId = $"{guild.Id}",
                CommandName = command.Name,
            }
        );

        return new GuildCommandDisabled(IsDisabled: disabled, WasCacheHit: false);
    }
}
