using System.Text.RegularExpressions;

namespace TaylorBot.Net.MessagesTracker.Domain;

public class WordCounter
{
    private readonly Regex regex = new(@"\s+");

    public int CountWords(string input)
    {
        var matchesCount = regex.Matches(input).Count;

        return matchesCount != 0 ? matchesCount + 1 : 1;
    }
}
