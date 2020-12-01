using Discord.Commands;
using StackExchange.Redis;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class DisabledCommandRedisCacheRepository : IDisabledCommandRepository
    {
        private static readonly string Key = "disabled-command-messages";

        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly DisabledCommandPostgresRepository _disabledCommandPostgresRepository;

        public DisabledCommandRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, DisabledCommandPostgresRepository disabledCommandPostgresRepository)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _disabledCommandPostgresRepository = disabledCommandPostgresRepository;
        }

        public async ValueTask<string> InsertOrGetCommandDisabledMessageAsync(CommandInfo command)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var message = await redis.HashGetAsync(Key, command.Aliases.First());

            if (!message.HasValue)
            {
                var disabledMessage = await _disabledCommandPostgresRepository.InsertOrGetCommandDisabledMessageAsync(command);
                await redis.HashSetAsync(Key, command.Aliases.First(), disabledMessage);
                return disabledMessage;
            }

            return message;
        }

        public async ValueTask EnableGloballyAsync(string commandName)
        {
            await _disabledCommandPostgresRepository.EnableGloballyAsync(commandName);

            var redis = _connectionMultiplexer.GetDatabase();
            await redis.HashSetAsync(Key, commandName, string.Empty);
        }

        public async ValueTask<string> DisableGloballyAsync(string commandName, string disabledMessage)
        {
            var storedMessage = await _disabledCommandPostgresRepository.DisableGloballyAsync(commandName, disabledMessage);

            var redis = _connectionMultiplexer.GetDatabase();
            await redis.HashSetAsync(Key, commandName, storedMessage);

            return storedMessage;
        }
    }
}
