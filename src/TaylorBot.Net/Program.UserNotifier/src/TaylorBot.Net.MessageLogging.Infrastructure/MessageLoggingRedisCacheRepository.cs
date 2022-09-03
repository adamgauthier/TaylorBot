using Discord;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.MessageLogging.Domain.TextChannel;

namespace TaylorBot.Net.MessageLogging.Infrastructure
{
    public class MessageLoggingRedisCacheRepository : IMessageLoggingChannelRepository
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly MessageLoggingChannelPostgresRepository _messageLoggingChannelPostgresRepository;

        public MessageLoggingRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, MessageLoggingChannelPostgresRepository messageLoggingChannelPostgresRepository)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _messageLoggingChannelPostgresRepository = messageLoggingChannelPostgresRepository;
        }

        public async ValueTask<LogChannel?> GetDeletedLogsChannelForGuildAsync(IGuild guild)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var key = $"deleted-logs:guild:{guild.Id}";
            var logChannelId = await redis.StringGetAsync(key);

            if (logChannelId.IsNull)
            {
                var logChannel = await _messageLoggingChannelPostgresRepository.GetDeletedLogsChannelForGuildAsync(guild);
                await redis.StringSetAsync(key, logChannel == null ? string.Empty : logChannel.ChannelId.ToString(), TimeSpan.FromMinutes(5));
                return logChannel;
            }

            return logChannelId == string.Empty ? null : new LogChannel(new SnowflakeId(logChannelId.ToString()));
        }

        public async ValueTask<LogChannel?> GetEditedLogsChannelForGuildAsync(IGuild guild)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var key = $"edited-logs:guild:{guild.Id}";
            var logChannelId = await redis.StringGetAsync(key);

            if (logChannelId.IsNull)
            {
                var logChannel = await _messageLoggingChannelPostgresRepository.GetEditedLogsChannelForGuildAsync(guild);
                await redis.StringSetAsync(key, logChannel == null ? string.Empty : logChannel.ChannelId.ToString(), TimeSpan.FromMinutes(5));
                return logChannel;
            }

            return logChannelId == string.Empty ? null : new LogChannel(new SnowflakeId(logChannelId.ToString()));
        }
    }
}
