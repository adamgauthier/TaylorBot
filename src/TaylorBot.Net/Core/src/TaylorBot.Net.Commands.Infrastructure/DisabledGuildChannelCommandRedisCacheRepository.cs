using StackExchange.Redis;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure;

public class DisabledGuildChannelCommandRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, DisabledGuildChannelCommandPostgresRepository disabledGuildChannelCommandPostgresRepository) : IDisabledGuildChannelCommandRepository
{
    private static string GetKey(CommandGuild guild, CommandChannel channel) =>
        $"enabled-commands:guild:{guild.Id}:channel:{channel.Id}";

    public async ValueTask DisableInAsync(CommandChannel channel, CommandGuild guild, string commandName)
    {
        await disabledGuildChannelCommandPostgresRepository.DisableInAsync(channel, guild, commandName);
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(guild, channel);
        await redis.HashSetAsync(key, commandName, false);
        await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
    }

    public async ValueTask EnableInAsync(CommandChannel channel, CommandGuild guild, string commandName)
    {
        await disabledGuildChannelCommandPostgresRepository.EnableInAsync(channel, guild, commandName);
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(guild, channel);
        await redis.HashSetAsync(key, commandName, true);
        await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
    }

    public async ValueTask<bool> IsGuildChannelCommandDisabledAsync(CommandChannel channel, CommandGuild guild, CommandMetadata command)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetKey(guild, channel);
        var isEnabled = await redis.HashGetAsync(key, command.Name);

        if (!isEnabled.HasValue)
        {
            var isDisabled = await disabledGuildChannelCommandPostgresRepository.IsGuildChannelCommandDisabledAsync(channel, guild, command);
            await redis.HashSetAsync(key, command.Name, !isDisabled);
            await redis.KeyExpireAsync(key, TimeSpan.FromHours(6));
            return isDisabled;
        }

        return !(bool)isEnabled;
    }
}
