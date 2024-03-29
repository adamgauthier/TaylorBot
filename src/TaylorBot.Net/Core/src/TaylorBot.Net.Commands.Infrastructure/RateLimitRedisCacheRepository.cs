﻿using StackExchange.Redis;

namespace TaylorBot.Net.Commands.Infrastructure;

public class RateLimitInMemoryRepository : IRateLimitRepository
{
    private readonly Dictionary<string, uint> _dailyUsage = [];

    public ValueTask<long> IncrementUsageAsync(string key)
    {
        var dailyUseCount = _dailyUsage.GetValueOrDefault(key, 0u) + 1;
        _dailyUsage[key] = dailyUseCount;
        return new ValueTask<long>(dailyUseCount);
    }
}

public class RateLimitRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer) : IRateLimitRepository
{
    public async ValueTask<long> IncrementUsageAsync(string key)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var dailyUseCount = await redis.StringIncrementAsync(key);
        await redis.KeyExpireAsync(key, TimeSpan.FromHours(25));
        return dailyUseCount;
    }
}
