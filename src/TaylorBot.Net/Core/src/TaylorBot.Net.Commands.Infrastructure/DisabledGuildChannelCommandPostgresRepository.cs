using Dapper;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure;

public class DisabledGuildChannelCommandPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IDisabledGuildChannelCommandRepository
{
    public async ValueTask DisableInAsync(CommandChannel channel, CommandGuild guild, string commandName)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO guilds.channel_commands(guild_id, channel_id, command_id)
            VALUES(@GuildId, @ChannelId, @CommandId) ON CONFLICT DO NOTHING;
            """,
            new
            {
                GuildId = $"{guild.Id}",
                ChannelId = $"{channel.Id}",
                CommandId = commandName,
            }
        );
    }

    public async ValueTask EnableInAsync(CommandChannel channel, CommandGuild guild, string commandName)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            DELETE FROM guilds.channel_commands
            WHERE guild_id = @GuildId AND channel_id = @ChannelId AND command_id = @CommandId;
            """,
            new
            {
                GuildId = $"{guild.Id}",
                ChannelId = $"{channel.Id}",
                CommandId = commandName,
            }
        );
    }

    public async ValueTask<bool> IsGuildChannelCommandDisabledAsync(CommandChannel channel, CommandGuild guild, CommandMetadata command)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var disabled = await connection.QuerySingleOrDefaultAsync<bool>(
            """
            SELECT EXISTS(
                SELECT 1 FROM guilds.channel_commands
                WHERE guild_id = @GuildId AND channel_id = @ChannelId AND command_id = @CommandId
            );
            """,
            new
            {
                GuildId = guild.Id.ToString(),
                ChannelId = channel.Id.ToString(),
                CommandId = command.Name,
            }
        );

        return disabled;
    }
}
