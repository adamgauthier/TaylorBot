using Discord;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Infrastructure;

public class CommandPrefixRedisCacheRepository : ICommandPrefixRepository
{
    private readonly ConnectionMultiplexer _connectionMultiplexer;
    private readonly CommandPrefixPostgresRepository _commandPrefixPostgresRepository;

    public CommandPrefixRedisCacheRepository(ConnectionMultiplexer connectionMultiplexer, CommandPrefixPostgresRepository commandPrefixPostgresRepository)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _commandPrefixPostgresRepository = commandPrefixPostgresRepository;
    }

    private static string GetPrefixKey(IGuild guild) => $"prefix:guild:{guild.Id}";

    public async ValueTask<CommandPrefix> GetOrInsertGuildPrefixAsync(IGuild guild)
    {
        var redis = _connectionMultiplexer.GetDatabase();
        var key = GetPrefixKey(guild);
        var cachedPrefix = await redis.StringGetAsync(key);

        if (!cachedPrefix.HasValue)
        {
            var result = await _commandPrefixPostgresRepository.GetOrInsertGuildPrefixAsync(guild);
            await redis.StringSetAsync(key, result.Prefix);
            return result;
        }

        return new CommandPrefix(new(WasAdded: false, WasGuildNameChanged: false, PreviousGuildName: null), $"{cachedPrefix}");
    }

    public async ValueTask ChangeGuildPrefixAsync(IGuild guild, string prefix)
    {
        await _commandPrefixPostgresRepository.ChangeGuildPrefixAsync(guild, prefix);

        var redis = _connectionMultiplexer.GetDatabase();
        await redis.StringSetAsync(GetPrefixKey(guild), prefix);
    }
}
