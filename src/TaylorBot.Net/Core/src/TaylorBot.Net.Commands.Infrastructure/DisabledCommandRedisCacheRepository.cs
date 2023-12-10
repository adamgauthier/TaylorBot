using StackExchange.Redis;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure;

public class DisabledCommandRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, DisabledCommandPostgresRepository disabledCommandPostgresRepository) : IDisabledCommandRepository
{
    private static readonly string Key = "disabled-command-messages";

    public async ValueTask<string> InsertOrGetCommandDisabledMessageAsync(CommandMetadata command)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var message = await redis.HashGetAsync(Key, command.Name);

        if (message.IsNull)
        {
            var disabledMessage = await disabledCommandPostgresRepository.InsertOrGetCommandDisabledMessageAsync(command);
            await redis.HashSetAsync(Key, command.Name, disabledMessage);
            return disabledMessage;
        }

        return message.ToString();
    }

    public async ValueTask EnableGloballyAsync(string commandName)
    {
        await disabledCommandPostgresRepository.EnableGloballyAsync(commandName);

        var redis = connectionMultiplexer.GetDatabase();
        await redis.HashSetAsync(Key, commandName, string.Empty);
    }

    public async ValueTask<string> DisableGloballyAsync(string commandName, string disabledMessage)
    {
        var storedMessage = await disabledCommandPostgresRepository.DisableGloballyAsync(commandName, disabledMessage);

        var redis = connectionMultiplexer.GetDatabase();
        await redis.HashSetAsync(Key, commandName, storedMessage);

        return storedMessage;
    }
}
