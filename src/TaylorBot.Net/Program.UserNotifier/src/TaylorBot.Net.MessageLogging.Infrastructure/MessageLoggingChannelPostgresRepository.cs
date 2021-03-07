﻿using Dapper;
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

        private class LogChannelDto
        {
            public string deleted_log_channel_id { get; set; } = null!;
        }

        public async ValueTask<LogChannel?> GetDeletedLogsChannelForGuildAsync(IGuild guild)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var logChannel = await connection.QuerySingleOrDefaultAsync<LogChannelDto?>(
                @"SELECT deleted_log_channel_id FROM plus.deleted_log_channels
                INNER JOIN plus.plus_guilds ON plus.deleted_log_channels.guild_id = plus.plus_guilds.guild_id
                WHERE plus.deleted_log_channels.guild_id = @GuildId AND state = 'enabled';",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );

            return logChannel != null ? new LogChannel(new SnowflakeId(logChannel.deleted_log_channel_id)) : null;
        }
    }
}
