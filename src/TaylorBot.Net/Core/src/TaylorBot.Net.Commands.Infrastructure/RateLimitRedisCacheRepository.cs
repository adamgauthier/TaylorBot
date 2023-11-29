using StackExchange.Redis;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class RateLimitInMemoryRepository : IRateLimitRepository
    {
        private readonly Dictionary<string, uint> _dailyUsage = new();

        public ValueTask<long> IncrementUsageAsync(string key)
        {
            var dailyUseCount = _dailyUsage.GetValueOrDefault(key, 0u) + 1;
            _dailyUsage[key] = dailyUseCount;
            return new ValueTask<long>(dailyUseCount);
        }
    }

    public class RateLimitRedisCacheRepository : IRateLimitRepository
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public RateLimitRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async ValueTask<long> IncrementUsageAsync(string key)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var dailyUseCount = await redis.StringIncrementAsync(key);
            await redis.KeyExpireAsync(key, TimeSpan.FromHours(25));
            return dailyUseCount;
        }
    }
}
