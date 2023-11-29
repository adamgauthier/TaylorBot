using Discord;
using StackExchange.Redis;
using System.Text.Json;
using TaylorBot.Net.MessageLogging.Domain.TextChannel;

namespace TaylorBot.Net.MessageLogging.Infrastructure;

public class MessageLoggingRedisCacheRepository : IMessageLoggingChannelRepository
{
    private readonly ConnectionMultiplexer _connectionMultiplexer;
    private readonly MessageLoggingChannelPostgresRepository _messageLoggingChannelPostgresRepository;

    public MessageLoggingRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, MessageLoggingChannelPostgresRepository messageLoggingChannelPostgresRepository)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _messageLoggingChannelPostgresRepository = messageLoggingChannelPostgresRepository;
    }

    public async ValueTask<MessageLogChannel?> GetDeletedLogsChannelForGuildAsync(IGuild guild)
    {
        var redis = _connectionMultiplexer.GetDatabase();
        var key = $"deleted-logs:guild:{guild.Id}";
        var cachedLogChannel = await redis.StringGetAsync(key);

        if (cachedLogChannel.IsNull)
        {
            var logChannel = await _messageLoggingChannelPostgresRepository.GetDeletedLogsChannelForGuildAsync(guild);
            await redis.StringSetAsync(key, logChannel == null ? string.Empty : $"{logChannel.ChannelId}/{JsonSerializer.Serialize(logChannel.CacheExpiry)}", TimeSpan.FromMinutes(5));
            return logChannel;
        }

        if (cachedLogChannel == string.Empty)
        {
            return null;
        }

        var parts = $"{cachedLogChannel}".Split('/');

        if (parts.Length > 1)
        {
            return new(parts[0], JsonSerializer.Deserialize<TimeSpan>(parts[1]));
        }
        else
        {
            return new(parts[0], null);
        }
    }

    public async ValueTask<MessageLogChannel?> GetEditedLogsChannelForGuildAsync(IGuild guild)
    {
        var redis = _connectionMultiplexer.GetDatabase();
        var key = $"edited-logs:guild:{guild.Id}";
        var cachedLogChannel = await redis.StringGetAsync(key);

        if (cachedLogChannel.IsNull)
        {
            var logChannel = await _messageLoggingChannelPostgresRepository.GetEditedLogsChannelForGuildAsync(guild);
            await redis.StringSetAsync(key, logChannel == null ? string.Empty : $"{logChannel.ChannelId}/{JsonSerializer.Serialize(logChannel.CacheExpiry)}", TimeSpan.FromMinutes(5));
            return logChannel;
        }

        if (cachedLogChannel == string.Empty)
        {
            return null;
        }

        var parts = $"{cachedLogChannel}".Split('/');

        if (parts.Length > 1)
        {
            return new(parts[0], JsonSerializer.Deserialize<TimeSpan>(parts[1]));
        }
        else
        {
            return new(parts[0], null);
        }
    }
}
