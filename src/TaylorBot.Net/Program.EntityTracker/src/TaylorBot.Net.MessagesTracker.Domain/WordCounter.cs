using System.Text.RegularExpressions;

namespace TaylorBot.Net.MessagesTracker.Domain;

public partial class WordCounter
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    private static readonly Regex Space = WhitespaceRegex();

    public int CountWords(string input)
    {
        var matchesCount = Space.Matches(input).Count;

        return matchesCount != 0 ? matchesCount + 1 : 1;
    }
}
