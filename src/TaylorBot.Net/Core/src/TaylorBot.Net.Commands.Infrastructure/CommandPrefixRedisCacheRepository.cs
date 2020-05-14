using Discord;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class CommandPrefixRedisCacheRepository : ICommandPrefixRepository
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly CommandPrefixPostgresRepository _commandPrefixPostgresRepository;

        public CommandPrefixRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, CommandPrefixPostgresRepository commandPrefixPostgresRepository)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _commandPrefixPostgresRepository = commandPrefixPostgresRepository;
        }

        public async Task<string> GetOrInsertGuildPrefixAsync(IGuild guild)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var key = $"prefix:guild:{guild.Id}";
            var cachedPrefix = await redis.StringGetAsync(key);

            if (!cachedPrefix.HasValue)
            {
                var prefix = await _commandPrefixPostgresRepository.GetOrInsertGuildPrefixAsync(guild);
                await redis.StringSetAsync(key, prefix);
                return prefix;
            }

            return cachedPrefix;
        }
    }
}
