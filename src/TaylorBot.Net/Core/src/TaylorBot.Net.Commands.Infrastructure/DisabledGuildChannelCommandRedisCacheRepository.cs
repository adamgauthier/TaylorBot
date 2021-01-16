using Discord;
using Discord.Commands;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class DisabledGuildChannelCommandRedisCacheRepository : IDisabledGuildChannelCommandRepository
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly DisabledGuildChannelCommandPostgresRepository _disabledGuildChannelCommandPostgresRepository;

        public DisabledGuildChannelCommandRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, DisabledGuildChannelCommandPostgresRepository disabledGuildChannelCommandPostgresRepository)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _disabledGuildChannelCommandPostgresRepository = disabledGuildChannelCommandPostgresRepository;
        }

        public async ValueTask<bool> IsGuildChannelCommandDisabledAsync(ITextChannel textChannel, CommandInfo command)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var key = $"enabled-commands:guild:{textChannel.GuildId}:channel:{textChannel.Id}";
            var isEnabled = await redis.HashGetAsync(key, command.Aliases.First());

            if (!isEnabled.HasValue)
            {
                var isDisabled = await _disabledGuildChannelCommandPostgresRepository.IsGuildChannelCommandDisabledAsync(textChannel, command);
                await redis.HashSetAsync(key, command.Aliases.First(), !isDisabled);
                await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
                return isDisabled;
            }

            return !(bool)isEnabled;
        }
    }
}
