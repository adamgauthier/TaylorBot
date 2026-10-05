using System.Text.RegularExpressions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands;

/// <summary>
/// Formats command mentions, awaiting missing IDs only for prefix commands or interactions with
/// <see cref="RunContext.WasAcknowledged"/> set. Matching interaction IDs and known cached IDs
/// return immediately; stale cache entries refresh in the background.
/// </summary>
/// <remarks>
/// Before acknowledgement, including errors while opening a modal, unresolved mentions fall back
/// immediately and refresh in the background. Global and guild mentions share this policy and the
/// repository's bounded waits, throttling, and warning behavior.
/// Background notifications use <see cref="SlashCommandMentioner"/> directly and may await missing IDs.
/// Pass interpolated messages to <see cref="FormatAsync"/> with <see cref="Slash"/> or
/// <see cref="GuildSlash"/> references. Capture reusable fragments as <see cref="FormattableString"/>
/// rather than string so their references are resolved with the enclosing message.
/// Use <see cref="Join"/> to compose lists of captured fragments without formatting them early.
/// </remarks>
public partial class CommandMentioner(SlashCommandMentioner mention)
{
    public CommandReference Command(Command command) => command.Metadata.IsSlashCommand
        ? Slash(command.Metadata.Name)
        : mention.Prefix(command.Metadata.Name);

    public CommandReference Slash(string name) => mention.Slash(name);

    public CommandReference GuildSlash(string name) => mention.GuildSlash(name);

    public FormattableString Join(string separator, IEnumerable<FormattableString> fragments) => mention.Join(separator, fragments);

    public Task<string> FormatAsync(RunContext context, FormattableString text) =>
        mention.FormatAsync(text,
            waitForRefresh: context.PrefixCommand != null || context.WasAcknowledged,
            guildId: context.Guild?.Id,
            interactionCommand: context.SlashCommand is { } command ? (command.Name, new SnowflakeId(command.Id)) : null);

    [GeneratedRegex(@"`/([^`]+)`")]
    private static partial Regex SlashCommandRegex();

    public async Task<string> ReplaceSlashCommandMentionsAsync(string input, RunContext context)
    {
        var regex = SlashCommandRegex();
        var names = regex.Matches(input).Select(match => match.Groups[1].Value).Distinct(StringComparer.Ordinal);
        var replacements = (await Task.WhenAll(names.Select(async name =>
            KeyValuePair.Create(name, await FormatAsync(context, $"{Slash(name)}"))))).ToDictionary();
        return regex.Replace(input, match => replacements[match.Groups[1].Value]);
    }
}
