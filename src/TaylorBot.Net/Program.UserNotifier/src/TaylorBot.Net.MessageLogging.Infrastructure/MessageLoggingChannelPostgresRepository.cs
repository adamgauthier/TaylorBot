using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.MessageLogging.Domain.TextChannel;

namespace TaylorBot.Net.MessageLogging.Infrastructure
{
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
        }

        public async ValueTask<LogChannel?> GetDeletedLogsChannelForGuildAsync(IGuild guild)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var logChannel = await connection.QuerySingleOrDefaultAsync<DeletedLogChannelDto?>(
                @"SELECT deleted_log_channel_id FROM plus.deleted_log_channels
                WHERE guild_id = @GuildId AND EXISTS (
                    SELECT FROM plus.plus_guilds
                    WHERE state = 'enabled' AND plus.deleted_log_channels.guild_id = plus.plus_guilds.guild_id
                );",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );

            return logChannel != null ? new LogChannel(new SnowflakeId(logChannel.deleted_log_channel_id)) : null;
        }

        private class EditedLogChannelDto
        {
            public string edited_log_channel_id { get; set; } = null!;
        }

        public async ValueTask<LogChannel?> GetEditedLogsChannelForGuildAsync(IGuild guild)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var logChannel = await connection.QuerySingleOrDefaultAsync<EditedLogChannelDto?>(
                @"SELECT edited_log_channel_id FROM plus.edited_log_channels
                WHERE guild_id = @GuildId AND EXISTS (
                    SELECT FROM plus.plus_guilds
                    WHERE state = 'enabled' AND plus.edited_log_channels.guild_id = plus.plus_guilds.guild_id
                );",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );

            return logChannel != null ? new LogChannel(new SnowflakeId(logChannel.edited_log_channel_id)) : null;
        }
    }
}
