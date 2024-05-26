using StackExchange.Redis;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Infrastructure;

public class DisabledGuildChannelCommandRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, DisabledGuildChannelCommandPostgresRepository disabledGuildChannelCommandPostgresRepository) : IDisabledGuildChannelCommandRepository
{
    private static string GetKey(GuildTextChannel channel) =>
        $"enabled-commands:guild:{channel.GuildId}:channel:{channel.Id}";

    public async ValueTask DisableInAsync(GuildTextChannel channel, string commandName)
    {
        await disabledGuildChannelCommandPostgresRepository.DisableInAsync(channel, commandName);
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(channel);
        await redis.HashSetAsync(key, commandName, false);
        await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
    }

    public async ValueTask EnableInAsync(GuildTextChannel channel, string commandName)
    {
        await disabledGuildChannelCommandPostgresRepository.EnableInAsync(channel, commandName);
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(channel);
        await redis.HashSetAsync(key, commandName, true);
        await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
    }

    public async ValueTask<bool> IsGuildChannelCommandDisabledAsync(GuildTextChannel channel, CommandMetadata command)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(channel);
        var isEnabled = await redis.HashGetAsync(key, command.Name);

        if (!isEnabled.HasValue)
        {
            var isDisabled = await disabledGuildChannelCommandPostgresRepository.IsGuildChannelCommandDisabledAsync(channel, command);
            await redis.HashSetAsync(key, command.Name, !isDisabled);
            await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
            return isDisabled;
        }

        return !(bool)isEnabled;
    }
}
