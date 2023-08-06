using Discord;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure;

public class DisabledGuildCommandRedisCacheRepository : IDisabledGuildCommandRepository
{
    private readonly ConnectionMultiplexer _connectionMultiplexer;
    private readonly DisabledGuildCommandPostgresRepository _disabledGuildCommandPostgresRepository;

    public DisabledGuildCommandRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, DisabledGuildCommandPostgresRepository disabledGuildCommandPostgresRepository)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _disabledGuildCommandPostgresRepository = disabledGuildCommandPostgresRepository;
    }

    private static string GetKey(IGuild guild) => $"enabled-commands:guild:{guild.Id}";

    public async ValueTask DisableInAsync(IGuild guild, string commandName)
    {
        await _disabledGuildCommandPostgresRepository.DisableInAsync(guild, commandName);
        var redis = _connectionMultiplexer.GetDatabase();
        var key = GetKey(guild);
        await redis.HashSetAsync(key, commandName, false);
        await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
    }

    public async ValueTask EnableInAsync(IGuild guild, string commandName)
    {
        await _disabledGuildCommandPostgresRepository.EnableInAsync(guild, commandName);
        var redis = _connectionMultiplexer.GetDatabase();
        var key = GetKey(guild);
        await redis.HashSetAsync(key, commandName, true);
        await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
    }

    public async ValueTask<GuildCommandDisabled> IsGuildCommandDisabledAsync(IGuild guild, CommandMetadata command)
    {
        var redis = _connectionMultiplexer.GetDatabase();
        var key = GetKey(guild);
        var isEnabled = await redis.HashGetAsync(key, command.Name);

        if (!isEnabled.HasValue)
        {
            var result = await _disabledGuildCommandPostgresRepository.IsGuildCommandDisabledAsync(guild, command);
            await redis.HashSetAsync(key, command.Name, !result.IsDisabled);
            await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
            return result;
        }

        return new GuildCommandDisabled(IsDisabled: !(bool)isEnabled, WasCacheHit: true);
    }
}
