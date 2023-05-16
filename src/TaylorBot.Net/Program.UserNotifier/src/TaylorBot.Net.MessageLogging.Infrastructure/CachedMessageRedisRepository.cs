using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.MessageLogging.Infrastructure;

public class CachedMessageRedisRepository : ICachedMessageRepository
{
    private readonly ConnectionMultiplexer _connectionMultiplexer;

    public CachedMessageRedisRepository(ConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    private static string GetKey(string id)
    {
        return $"message-content:{id}";
    }

    public async ValueTask<TaylorBotCachedMessageData?> GetMessageDataAsync(SnowflakeId messageId)
    {
        var redis = _connectionMultiplexer.GetDatabase();
        var key = GetKey(messageId.ToString());
        var cachedJson = (string?)await redis.StringGetAsync(key);

        if (!string.IsNullOrEmpty(cachedJson))
        {
            return JsonSerializer.Deserialize<TaylorBotCachedMessageData>(cachedJson);
        }
        else
        {
            return null;
        }
    }

    public async ValueTask SaveMessageAsync(SnowflakeId messageId, TimeSpan expiry, TaylorBotCachedMessageData data)
    {
        var redis = _connectionMultiplexer.GetDatabase();
        var key = GetKey(messageId.ToString());
        await redis.StringSetAsync(key, JsonSerializer.Serialize(data), expiry);
    }
}
