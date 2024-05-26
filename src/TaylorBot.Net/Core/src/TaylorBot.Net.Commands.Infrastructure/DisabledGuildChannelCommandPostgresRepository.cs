using Dapper;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Infrastructure;

public class DisabledGuildChannelCommandPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IDisabledGuildChannelCommandRepository
{
    public async ValueTask DisableInAsync(GuildTextChannel channel, string commandName)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO guilds.channel_commands(guild_id, channel_id, command_id)
            VALUES(@GuildId, @ChannelId, @CommandId) ON CONFLICT DO NOTHING;
            """,
            new
            {
                GuildId = $"{channel.GuildId}",
                ChannelId = $"{channel.Id}",
                CommandId = commandName,
            }
        );
    }

    public async ValueTask EnableInAsync(GuildTextChannel channel, string commandName)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            DELETE FROM guilds.channel_commands
            WHERE guild_id = @GuildId AND channel_id = @ChannelId AND command_id = @CommandId;
            """,
            new
            {
                GuildId = $"{channel.GuildId}",
                ChannelId = $"{channel.Id}",
                CommandId = commandName,
            }
        );
    }

    public async ValueTask<bool> IsGuildChannelCommandDisabledAsync(GuildTextChannel channel, CommandMetadata command)
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
                GuildId = $"{channel.GuildId}",
                ChannelId = $"{channel.Id}",
                CommandId = command.Name,
            }
        );

        return disabled;
    }
}
