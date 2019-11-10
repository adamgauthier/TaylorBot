using Discord.Commands;
using StackExchange.Redis;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class DisabledCommandRedisCacheRepository : IDisabledCommandRepository
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly DisabledCommandPostgresRepository _disabledCommandPostgresRepository;

        public DisabledCommandRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, DisabledCommandPostgresRepository disabledCommandPostgresRepository)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _disabledCommandPostgresRepository = disabledCommandPostgresRepository;
        }

        public async Task<bool> InsertOrGetIsCommandDisabledAsync(CommandInfo command)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var key = "enabled-commands";
            var isEnabled = await redis.HashGetAsync(key, command.Name);

            if (!isEnabled.HasValue)
            {
                var isDisabled = await _disabledCommandPostgresRepository.InsertOrGetIsCommandDisabledAsync(command);
                await redis.HashSetAsync(key, command.Name, !isDisabled);
                return isDisabled;
            }

            return !(bool)isEnabled;
        }
    }
}
