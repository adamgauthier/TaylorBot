using StackExchange.Redis;
using System.Text.Json;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.MessageLogging.Infrastructure;

public class CachedMessageRedisRepository(ConnectionMultiplexer connectionMultiplexer) : ICachedMessageRepository
{
    private static string GetKey(string id)
    {
        return $"message-content:{id}";
    }

    public async ValueTask<TaylorBotCachedMessageData?> GetMessageDataAsync(SnowflakeId messageId)
    {
        var redis = connectionMultiplexer.GetDatabase();
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
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(messageId.ToString());
        await redis.StringSetAsync(key, JsonSerializer.Serialize(data), expiry);
    }
}
