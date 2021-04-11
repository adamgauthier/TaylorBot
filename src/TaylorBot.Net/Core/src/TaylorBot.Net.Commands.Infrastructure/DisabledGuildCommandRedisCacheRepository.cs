using Discord;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class DisabledGuildCommandRedisCacheRepository : IDisabledGuildCommandRepository
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly DisabledGuildCommandPostgresRepository _disabledGuildCommandPostgresRepository;

        public DisabledGuildCommandRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, DisabledGuildCommandPostgresRepository disabledGuildCommandPostgresRepository)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _disabledGuildCommandPostgresRepository = disabledGuildCommandPostgresRepository;
        }

        public async ValueTask<bool> IsGuildCommandDisabledAsync(IGuild guild, CommandMetadata command)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var key = $"enabled-commands:guild:{guild.Id}";
            var isEnabled = await redis.HashGetAsync(key, command.Name);

            if (!isEnabled.HasValue)
            {
                var isDisabled = await _disabledGuildCommandPostgresRepository.IsGuildCommandDisabledAsync(guild, command);
                await redis.HashSetAsync(key, command.Name, !isDisabled);
                await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
                return isDisabled;
            }

            return !(bool)isEnabled;
        }
    }
}
