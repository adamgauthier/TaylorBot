using Dapper;
using Discord;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.MessageLogging.Domain.TextChannel;

namespace TaylorBot.Net.MessageLogging.Infrastructure;

public class MessageLoggingChannelPostgresRepository : IMessageLoggingChannelRepository
{
    private readonly PostgresConnectionFactory _postgresConnectionFactory;

    public MessageLoggingChannelPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
    {
        _postgresConnectionFactory = postgresConnectionFactory;
    }

    private class DeletedLogChannelDto
    {
        public string deleted_log_channel_id { get; set; } = null!;
        public TimeSpan max_message_content_cache_expiry { get; set; }

        public MessageLogChannel ToMessageLog() => new(new(deleted_log_channel_id), max_message_content_cache_expiry);
    }

    public async ValueTask<MessageLogChannel?> GetDeletedLogsChannelForGuildAsync(IGuild guild)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var logChannel = await connection.QuerySingleOrDefaultAsync<DeletedLogChannelDto?>(
            """
            SELECT deleted_log_channel_id, MAX(message_content_cache_expiry) AS max_message_content_cache_expiry
            FROM plus.deleted_log_channels
            INNER JOIN plus.plus_guilds
            ON plus.deleted_log_channels.guild_id = plus.plus_guilds.guild_id
            WHERE plus_guilds.guild_id = @GuildId
            AND plus_guilds.state = 'enabled'
            GROUP BY deleted_log_channel_id;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        return logChannel?.ToMessageLog();
    }

    private class EditedLogChannelDto
    {
        public string edited_log_channel_id { get; set; } = null!;
        public TimeSpan max_message_content_cache_expiry { get; set; }

        public MessageLogChannel ToMessageLog() => new(new(edited_log_channel_id), max_message_content_cache_expiry);
    }

    public async ValueTask<MessageLogChannel?> GetEditedLogsChannelForGuildAsync(IGuild guild)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var logChannel = await connection.QuerySingleOrDefaultAsync<EditedLogChannelDto?>(
            """
            SELECT edited_log_channel_id, MAX(message_content_cache_expiry) AS max_message_content_cache_expiry
            FROM plus.edited_log_channels
            INNER JOIN plus.plus_guilds
            ON plus.edited_log_channels.guild_id = plus.plus_guilds.guild_id
            WHERE plus_guilds.guild_id = @GuildId
            AND plus_guilds.state = 'enabled'
            GROUP BY edited_log_channel_id;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        return logChannel?.ToMessageLog();
    }
}
