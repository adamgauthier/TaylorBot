using Microsoft.Extensions.Caching.Memory;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Core.Program;

public class ApplicationCommandsRepository(BackgroundTasks backgroundTasks, IMemoryCache memoryCache, Lazy<ITaylorBotClient> taylorBotClient) : IApplicationCommandsRepository
{
    private const string GlobalCacheKey = "global-application-commands";

    public SnowflakeId? GetCommandId(string name)
    {
        if (memoryCache.TryGetValue(GlobalCacheKey, out Dictionary<string, SnowflakeId>? commandsLookup))
        {
            return commandsLookup?.TryGetValue(name, out var commandId) == true ? commandId : null;
        }

        _ = backgroundTasks.Queue(CacheCommandsAsync, nameof(CacheCommandsAsync));
        return null;
    }

    public SnowflakeId? GetGuildCommandId(SnowflakeId guildId, string name)
    {
        var key = $"guild-application-commands-{guildId}";
        if (memoryCache.TryGetValue(key, out Dictionary<string, SnowflakeId>? commandsLookup))
        {
            return commandsLookup?.TryGetValue(name, out var commandId) == true ? commandId : null;
        }

        _ = backgroundTasks.Queue(() => CacheGuildCommandsAsync(guildId), nameof(CacheGuildCommandsAsync));
        return null;
    }

    public async Task CacheCommandsAsync()
    {
        _ = await memoryCache.GetOrCreateAsync(GlobalCacheKey, async entry =>
        {
            var commands = await taylorBotClient.Value.RestClient.GetGlobalApplicationCommands();
            return commands.ToDictionary(c => c.Name, c => new SnowflakeId(c.Id));
        },
        new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
        });
    }

    private async Task CacheGuildCommandsAsync(SnowflakeId guildId)
    {
        var key = $"guild-application-commands-{guildId}";
        _ = await memoryCache.GetOrCreateAsync(key, async entry =>
        {
            var guild = taylorBotClient.Value.ResolveRequiredGuild(guildId);
            var commands = await guild.GetApplicationCommandsAsync();
            return commands.ToDictionary(c => c.Name, c => new SnowflakeId(c.Id));
        },
        new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
        });
    }
}
