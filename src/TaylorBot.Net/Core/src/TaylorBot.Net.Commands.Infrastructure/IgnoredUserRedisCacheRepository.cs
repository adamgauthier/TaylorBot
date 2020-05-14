using Discord;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class IgnoredUserRedisCacheRepository : IIgnoredUserRepository
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IgnoredUserPostgresRepository _ignoredUserPostgresRepository;

        public IgnoredUserRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, IgnoredUserPostgresRepository ignoredUserPostgresRepository)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _ignoredUserPostgresRepository = ignoredUserPostgresRepository;
        }

        public async Task<GetUserIgnoreUntilResult> InsertOrGetUserIgnoreUntilAsync(IUser user)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var key = $"ignore-until:user:{user.Id}";
            var cachedIgnoreUntil = await redis.StringGetAsync(key);

            if (!cachedIgnoreUntil.HasValue)
            {
                var getUserIgnoreUntilResult = await _ignoredUserPostgresRepository.InsertOrGetUserIgnoreUntilAsync(user);
                await redis.StringSetAsync(
                    key,
                    getUserIgnoreUntilResult.IgnoreUntil.ToUnixTimeMilliseconds(),
                    TimeSpan.FromHours(1)
                );
                return getUserIgnoreUntilResult;
            }

            return new GetUserIgnoreUntilResult(
                ignoreUntil: DateTimeOffset.FromUnixTimeMilliseconds((long)cachedIgnoreUntil),
                wasAdded: false,
                wasUsernameChanged: false,
                previousUsername: null
            );
        }
    }
}
