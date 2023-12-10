using Discord;
using StackExchange.Redis;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure;

public class DisabledGuildCommandRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, DisabledGuildCommandPostgresRepository disabledGuildCommandPostgresRepository) : IDisabledGuildCommandRepository
{
    private static string GetKey(IGuild guild) => $"enabled-commands:guild:{guild.Id}";

    public async ValueTask DisableInAsync(IGuild guild, string commandName)
    {
        await disabledGuildCommandPostgresRepository.DisableInAsync(guild, commandName);
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(guild);
        await redis.HashSetAsync(key, commandName, false);
        await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
    }

    public async ValueTask EnableInAsync(IGuild guild, string commandName)
    {
        await disabledGuildCommandPostgresRepository.EnableInAsync(guild, commandName);
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(guild);
        await redis.HashSetAsync(key, commandName, true);
        await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
    }

    public async ValueTask<GuildCommandDisabled> IsGuildCommandDisabledAsync(IGuild guild, CommandMetadata command)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(guild);
        var isEnabled = await redis.HashGetAsync(key, command.Name);

        if (!isEnabled.HasValue)
        {
            var result = await disabledGuildCommandPostgresRepository.IsGuildCommandDisabledAsync(guild, command);
            await redis.HashSetAsync(key, command.Name, !result.IsDisabled);
            await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
            return result;
        }

        return new GuildCommandDisabled(IsDisabled: !(bool)isEnabled, WasCacheHit: true);
    }
}
