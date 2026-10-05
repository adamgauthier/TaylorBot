using System.Globalization;
using System.Runtime.CompilerServices;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.Client;

public interface IApplicationCommandsRepository
{
    ValueTask<SnowflakeId?> GetCommandIdAsync(string name, bool waitForRefresh);
    ValueTask<SnowflakeId?> GetGuildCommandIdAsync(SnowflakeId guildId, string name, bool waitForRefresh);
    Task CacheCommandsAsync();
}

/// <summary>
/// Formats captured message interpolations, resolving typed command references together.
/// Ordinary string arguments remain literal; nested <see cref="FormattableString"/> fragments are supported.
/// </summary>
/// <remarks>
/// Missing IDs may await a bounded, throttled refresh by default, suitable for background notifications.
/// Interaction callers must disable waiting before acknowledgement. Unresolved lookups are logged
/// and fall back to readable text; disabling waiting still permits a background refresh.
/// Matching interaction IDs and cached IDs return immediately; stale cache entries refresh in the background.
/// </remarks>
public class SlashCommandMentioner(IApplicationCommandsRepository commands)
{
    public CommandReference Slash(string name) => new(name, CommandReferenceKind.Slash);

    public CommandReference GuildSlash(string name) => new(name, CommandReferenceKind.GuildSlash);

    public CommandReference Prefix(string name) => new(name, CommandReferenceKind.Prefix);

    public FormattableString Join(string separator, IEnumerable<FormattableString> fragments)
    {
        var arguments = fragments.Cast<object?>().ToArray();
        var literalSeparator = separator.Replace("{", "{{", StringComparison.Ordinal).Replace("}", "}}", StringComparison.Ordinal);
        var format = string.Join(literalSeparator, Enumerable.Range(0, arguments.Length).Select(index => $"{{{index}}}"));
        return FormattableStringFactory.Create(format, arguments);
    }

    public async Task<string> FormatAsync(FormattableString text, bool waitForRefresh = true, SnowflakeId? guildId = null,
        (string Name, SnowflakeId Id)? interactionCommand = null)
    {
        var references = GetReferences(text).Distinct();
        var resolved = (await Task.WhenAll(references.Select(async reference =>
            KeyValuePair.Create(reference, await ResolveAsync(reference, waitForRefresh, guildId, interactionCommand))))).ToDictionary();
        return Render(text, resolved);
    }

    private async Task<string> ResolveAsync(CommandReference reference, bool waitForRefresh, SnowflakeId? guildId,
        (string Name, SnowflakeId Id)? interactionCommand)
    {
        if (reference.Kind == CommandReferenceKind.Prefix)
        {
            return $"**{reference.Name}**";
        }

        var root = reference.Name.Split(' ')[0];
        var knownId = interactionCommand is { } command && root == command.Name.Split(' ')[0] ? command.Id : null;
        var id = reference.Kind switch
        {
            CommandReferenceKind.Slash => knownId ?? await commands.GetCommandIdAsync(root, waitForRefresh),
            CommandReferenceKind.GuildSlash => guildId == null
                ? throw new InvalidOperationException("Guild command references require a guild context.")
                : knownId ?? await commands.GetGuildCommandIdAsync(guildId, root, waitForRefresh),
            _ => throw new InvalidOperationException($"Unknown command reference kind: {reference.Kind}."),
        };
        return id != null ? $"</{reference.Name}:{id}>" : $"**/{reference.Name}**";
    }

    private static IEnumerable<CommandReference> GetReferences(FormattableString text)
    {
        foreach (var argument in text.GetArguments())
        {
            if (argument is CommandReference reference)
            {
                yield return reference;
            }
            else if (argument is FormattableString nested)
            {
                foreach (var nestedReference in GetReferences(nested))
                {
                    yield return nestedReference;
                }
            }
        }
    }

    private static string Render(FormattableString text, IReadOnlyDictionary<CommandReference, string> resolved) =>
        string.Format(CultureInfo.CurrentCulture, text.Format, text.GetArguments().Select(argument => argument switch
        {
            CommandReference reference => resolved[reference],
            FormattableString nested => Render(nested, resolved),
            _ => argument,
        }).ToArray());
}

public enum CommandReferenceKind { Slash, GuildSlash, Prefix }

/// <summary>A command reference captured by a mentioner's <c>FormatAsync</c>, not a preformatted string.</summary>
public sealed record CommandReference(string Name, CommandReferenceKind Kind) : IFormattable
{
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) =>
        throw new FormatException("Command references must be rendered with FormatAsync.");
}
