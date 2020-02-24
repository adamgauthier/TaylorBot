using Discord;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class OnGoingCommandRedisRepository : IOngoingCommandRepository
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public OnGoingCommandRedisRepository(ConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        private string GetKey(IUser user) => $"ongoing-commands:user:{user.Id}";

        public async Task AddOngoingCommandAsync(IUser user)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var key = GetKey(user);
            await redis.StringIncrementAsync(key);
            await redis.KeyExpireAsync(key, TimeSpan.FromSeconds(10));
        }

        public async Task<bool> HasAnyOngoingCommandAsync(IUser user)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var key = GetKey(user);
            var ongoingCommands = await redis.StringGetAsync(key);

            return ongoingCommands.HasValue && (long)ongoingCommands > 0;
        }

        public async Task RemoveOngoingCommandAsync(IUser user)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var key = GetKey(user);
            await redis.StringDecrementAsync(key);
        }
    }
}
