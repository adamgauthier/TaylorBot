using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands;

public interface IApplicationCommandsRepository
{
    SnowflakeId? GetCommandId(string name);
    SnowflakeId? GetGuildCommandId(SnowflakeId guildId, string name);
    Task CacheCommandsAsync();
}

public class ApplicationCommandsRepository(TaskExceptionLogger taskExceptionLogger, IMemoryCache memoryCache, Lazy<ITaylorBotClient> taylorBotClient) : IApplicationCommandsRepository
{
    private const string GlobalCacheKey = "global-application-commands";

    public SnowflakeId? GetCommandId(string name)
    {
        if (memoryCache.TryGetValue(GlobalCacheKey, out Dictionary<string, SnowflakeId>? commandsLookup))
        {
            return commandsLookup?.TryGetValue(name, out var commandId) == true ? commandId : null;
        }

        _ = Task.Run(async () => await taskExceptionLogger.LogOnError(CacheCommandsAsync, nameof(CacheCommandsAsync)));
        return null;
    }

    public SnowflakeId? GetGuildCommandId(SnowflakeId guildId, string name)
    {
        var key = $"guild-application-commands-{guildId}";
        if (memoryCache.TryGetValue(key, out Dictionary<string, SnowflakeId>? commandsLookup))
        {
            return commandsLookup?.TryGetValue(name, out var commandId) == true ? commandId : null;
        }

        _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
            () => CacheGuildCommandsAsync(guildId), nameof(CacheGuildCommandsAsync)));
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

public partial class CommandMentioner(IApplicationCommandsRepository commands)
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

    public string GuildSlashCommand(string name, SnowflakeId guildId)
    {
        var rootName = name.Split(' ')[0];

        var id = commands.GetGuildCommandId(guildId, rootName);
        if (id != null)
        {
            return $"</{name}:{id}>";
        }

        return $"**/{name}**";
    }

    [GeneratedRegex(@"`/([^`]+)`")]
    private static partial Regex SlashCommandRegex();

    public string ReplaceSlashCommandMentions(string input)
    {
        return SlashCommandRegex().Replace(input, match => SlashCommand(match.Groups[1].Value));
    }
}
