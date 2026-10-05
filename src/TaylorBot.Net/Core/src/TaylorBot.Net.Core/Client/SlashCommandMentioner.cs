using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.Client;

public interface IApplicationCommandsRepository
{
    SnowflakeId? GetCommandId(string name);
    SnowflakeId? GetGuildCommandId(SnowflakeId guildId, string name);
    Task CacheCommandsAsync();
}

public class SlashCommandMentioner(IApplicationCommandsRepository commands)
{
    public async Task<string> SlashCommandAsync(string name)
    {
        await commands.CacheCommandsAsync();
        return SlashCommand(name);
    }

    public string SlashCommand(string name, SnowflakeId? fallbackId = null)
    {
        var id = commands.GetCommandId(name.Split(' ')[0]) ?? fallbackId;
        return id != null ? $"</{name}:{id}>" : $"**/{name}**";
    }

    public string GuildSlashCommand(string name, SnowflakeId guildId)
    {
        var id = commands.GetGuildCommandId(guildId, name.Split(' ')[0]);
        return id != null ? $"</{name}:{id}>" : $"**/{name}**";
    }
}
