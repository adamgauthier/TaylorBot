using Discord;
using StackExchange.Redis;

namespace TaylorBot.Net.Commands.Infrastructure;

public class CommandPrefixRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, CommandPrefixPostgresRepository commandPrefixPostgresRepository) : ICommandPrefixRepository
{
    private static string GetPrefixKey(IGuild guild) => $"prefix:guild:{guild.Id}";

    public async ValueTask<CommandPrefix> GetOrInsertGuildPrefixAsync(IGuild guild)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var key = GetPrefixKey(guild);
        var cachedPrefix = await redis.StringGetAsync(key);

        if (!cachedPrefix.HasValue)
        {
            var result = await commandPrefixPostgresRepository.GetOrInsertGuildPrefixAsync(guild);
            await redis.StringSetAsync(key, result.Prefix);
            return result;
        }

        return new CommandPrefix(new(WasAdded: false, WasGuildNameChanged: false, PreviousGuildName: null), $"{cachedPrefix}");
    }

    public async ValueTask ChangeGuildPrefixAsync(IGuild guild, string prefix)
    {
        await commandPrefixPostgresRepository.ChangeGuildPrefixAsync(guild, prefix);

        var redis = connectionMultiplexer.GetDatabase();
        await redis.StringSetAsync(GetPrefixKey(guild), prefix);
    }
}
