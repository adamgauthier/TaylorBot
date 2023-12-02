using Discord;
using StackExchange.Redis;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure;

public class DisabledGuildChannelCommandRedisCacheRepository : IDisabledGuildChannelCommandRepository
{
    private readonly ConnectionMultiplexer _connectionMultiplexer;
    private readonly DisabledGuildChannelCommandPostgresRepository _disabledGuildChannelCommandPostgresRepository;

    public DisabledGuildChannelCommandRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, DisabledGuildChannelCommandPostgresRepository disabledGuildChannelCommandPostgresRepository)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _disabledGuildChannelCommandPostgresRepository = disabledGuildChannelCommandPostgresRepository;
    }
    private static string GetKey(IGuild guild, MessageChannel channel) => $"enabled-commands:guild:{guild.Id}:channel:{channel.Id}";

    public async ValueTask DisableInAsync(MessageChannel channel, IGuild guild, string commandName)
    {
        await _disabledGuildChannelCommandPostgresRepository.DisableInAsync(channel, guild, commandName);
        var redis = _connectionMultiplexer.GetDatabase();
        var key = GetKey(guild, channel);
        await redis.HashSetAsync(key, commandName, false);
        await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
    }

    public async ValueTask EnableInAsync(MessageChannel channel, IGuild guild, string commandName)
    {
        await _disabledGuildChannelCommandPostgresRepository.EnableInAsync(channel, guild, commandName);
        var redis = _connectionMultiplexer.GetDatabase();
        var key = GetKey(guild, channel);
        await redis.HashSetAsync(key, commandName, true);
        await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
    }

    public async ValueTask<bool> IsGuildChannelCommandDisabledAsync(MessageChannel channel, IGuild guild, CommandMetadata command)
    {
        var redis = _connectionMultiplexer.GetDatabase();
        var key = GetKey(guild, channel);
        var isEnabled = await redis.HashGetAsync(key, command.Name);

        if (!isEnabled.HasValue)
        {
            var isDisabled = await _disabledGuildChannelCommandPostgresRepository.IsGuildChannelCommandDisabledAsync(channel, guild, command);
            await redis.HashSetAsync(key, command.Name, !isDisabled);
            await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
            return isDisabled;
        }

        return !(bool)isEnabled;
    }
}
