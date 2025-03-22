using Microsoft.Extensions.Caching.Memory;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands;

public interface IApplicationCommandsRepository
{
    SnowflakeId? GetCommandId(string name);
    Task CacheCommandsAsync();
}

public class ApplicationCommandsRepository(TaskExceptionLogger taskExceptionLogger, IMemoryCache memoryCache, Lazy<ITaylorBotClient> taylorBotClient) : IApplicationCommandsRepository
{
    private const string CacheKey = "global-application-commands";

    public SnowflakeId? GetCommandId(string name)
    {
        if (memoryCache.TryGetValue(CacheKey, out Dictionary<string, SnowflakeId>? commandsLookup))
        {
            return commandsLookup?.TryGetValue(name, out var commandId) == true ? commandId : null;
        }

        // Cache miss, fetch and cache in the background and return null for this call
        // Doing this in the background to avoid this method being async
        _ = Task.Run(async () => await taskExceptionLogger.LogOnError(CacheCommandsAsync, nameof(CacheCommandsAsync)));
        return null;
    }

    public async Task CacheCommandsAsync()
    {
        _ = await memoryCache.GetOrCreateAsync("global-application-commands", async entry =>
        {
            var commands = await taylorBotClient.Value.DiscordShardedClient.Rest.GetGlobalApplicationCommands();
            return commands.ToDictionary(c => c.Name, c => new SnowflakeId(c.Id));
        },
        new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
        });
    }
}

public class CommandMentioner(IApplicationCommandsRepository commands)
{
    public string Command(Command command, RunContext? context = null) => command.Metadata.IsSlashCommand
        ? SlashCommand(command.Metadata.Name, context)
        : $"**{command.Metadata.Name}**";

    public string SlashCommand(string name, RunContext? context = null)
    {
        var rootName = name.Split(' ')[0];

        var id = commands.GetCommandId(rootName);
        if (id != null)
        {
            return $"</{name}:{id}>";
        }

        return context?.SlashCommand != null && rootName == context.SlashCommand.Name.Split(' ')[0]
            ? $"</{name}:{context.SlashCommand.Id}>"
            : $"**/{name}**";
    }
}
