using Discord;
using StackExchange.Redis;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure;

public class IgnoredUserRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, IgnoredUserPostgresRepository ignoredUserPostgresRepository) : IIgnoredUserRepository
{
    private static string GetKey(IUser user) => $"ignore-until:user:{user.Id}";

    private static async ValueTask CacheAsync(IDatabase redis, string key, DateTimeOffset ignoreUntil)
    {
        await redis.StringSetAsync(
            key,
            ignoreUntil.ToUnixTimeMilliseconds(),
            TimeSpan.FromHours(1)
        );
    }

    public async ValueTask<GetUserIgnoreUntilResult> InsertOrGetUserIgnoreUntilAsync(IUser user, bool isBot)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(user);
        var cachedIgnoreUntil = await redis.StringGetAsync(key);

        if (!cachedIgnoreUntil.HasValue)
        {
            var getUserIgnoreUntilResult = await ignoredUserPostgresRepository.InsertOrGetUserIgnoreUntilAsync(user, isBot);
            await CacheAsync(redis, key, getUserIgnoreUntilResult.IgnoreUntil);
            return getUserIgnoreUntilResult;
        }

        return new GetUserIgnoreUntilResult(
            IgnoreUntil: DateTimeOffset.FromUnixTimeMilliseconds((long)cachedIgnoreUntil),
            WasAdded: false,
            WasUsernameChanged: false,
            PreviousUsername: null
        );
    }

    public async ValueTask IgnoreUntilAsync(IUser user, DateTimeOffset until)
    {
        await ignoredUserPostgresRepository.IgnoreUntilAsync(user, until);

        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(user);
        await CacheAsync(redis, key, until);
    }
}
