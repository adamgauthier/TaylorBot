using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.MessageLogging.Infrastructure;

public class CachedMessageInMemoryRepository : ICachedMessageRepository
{
    private readonly Dictionary<ulong, TaylorBotCachedMessageData> _cachedMessages = [];

    public ValueTask<TaylorBotCachedMessageData?> GetMessageDataAsync(SnowflakeId messageId)
    {
        if (_cachedMessages.TryGetValue(messageId.Id, out var value))
        {
            return new(value);
        }
        else
        {
            return new(result: null);
        }
    }

    public ValueTask SaveMessageAsync(SnowflakeId messageId, TimeSpan expiry, TaylorBotCachedMessageData data)
    {
        _cachedMessages[messageId.Id] = data;
        return new();
    }
}
