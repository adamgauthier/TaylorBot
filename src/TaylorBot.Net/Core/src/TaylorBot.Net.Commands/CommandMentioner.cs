using System.Text.RegularExpressions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands;

public partial class CommandMentioner(SlashCommandMentioner mention)
{
    public string Command(Command command, RunContext? context = null) => command.Metadata.IsSlashCommand
        ? SlashCommand(command.Metadata.Name, context)
        : $"**{command.Metadata.Name}**";

    public string SlashCommand(string name, RunContext? context = null)
    {
        var rootName = name.Split(' ')[0];

        return mention.SlashCommand(name, fallbackId: context?.SlashCommand != null && rootName == context.SlashCommand.Name.Split(' ')[0]
            ? new(context.SlashCommand.Id)
            : null);
    }

    public string GuildSlashCommand(string name, SnowflakeId guildId) => mention.GuildSlashCommand(name, guildId);

    [GeneratedRegex(@"`/([^`]+)`")]
    private static partial Regex SlashCommandRegex();

    public string ReplaceSlashCommandMentions(string input)
    {
        return SlashCommandRegex().Replace(input, match => SlashCommand(match.Groups[1].Value));
    }
}
