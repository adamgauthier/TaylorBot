using Discord.Commands;
using System.Text.RegularExpressions;

namespace TaylorBot.Net.Commands.Types;

public class Word(string value)
{
    public string Value { get; } = value;
}

public partial class WordTypeReader : TypeReader
{
    [GeneratedRegex(@"\s", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();

    private static readonly Regex Whitespace = WhitespaceRegex();

    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        if (Whitespace.IsMatch(input))
        {
            return Task.FromResult(TypeReaderResult.FromError(
                CommandError.ParseFailed, "Input value can't contain spaces or line breaks."
            ));
        }
        else
        {
            return Task.FromResult(TypeReaderResult.FromSuccess(new Word(input)));
        }
    }
}
