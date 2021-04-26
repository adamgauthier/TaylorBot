using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.MessageLogging.Infrastructure
{
    public class CachedMessageInMemoryRepository : ICachedMessageRepository
    {
        private readonly Dictionary<ulong, TaylorBotCachedMessageData> _cachedMessages = new();

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

        public ValueTask SaveMessageAsync(SnowflakeId messageId, TaylorBotCachedMessageData data)
        {
            _cachedMessages.Add(messageId.Id, data);
            return new();
        }
    }
}
