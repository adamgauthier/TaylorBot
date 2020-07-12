using Discord;
using StackExchange.Redis;
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

        public async ValueTask<LogChannel?> GetMessageLogChannelForGuildAsync(IGuild guild)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var key = "message-log-channels";
            var logChannelId = await redis.HashGetAsync(key, guild.Id.ToString());

            if (logChannelId.IsNull)
            {
                var logChannel = await _messageLoggingChannelPostgresRepository.GetMessageLogChannelForGuildAsync(guild);
                await redis.HashSetAsync(key, guild.Id.ToString(), logChannel == null ? string.Empty : logChannel.ChannelId.ToString());
                return logChannel;
            }

            return logChannelId == string.Empty ? null : new LogChannel(new SnowflakeId(logChannelId));
        }
    }
}
