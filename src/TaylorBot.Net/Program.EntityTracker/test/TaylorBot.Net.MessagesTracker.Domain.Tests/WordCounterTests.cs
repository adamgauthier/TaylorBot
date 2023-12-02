using FluentAssertions;
using Xunit;

namespace TaylorBot.Net.MessagesTracker.Domain.Tests;

public class WordCounterTests
{
    private readonly WordCounter wordCounter;

    public WordCounterTests()
    {
        wordCounter = new WordCounter();
    }

    [Fact]
    public void CountWords_When1Word_ThenReturns1()
    {
        var oneWord = "Hey";

        var wordCount = wordCounter.CountWords(oneWord);

        wordCount.Should().Be(1);
    }

    [Fact]
    public void CountWords_When5Words_ThenReturns5()
    {
        var fiveWords = "Hey guys, how are you?";

        var wordCount = wordCounter.CountWords(fiveWords);

        wordCount.Should().Be(5);
    }

    [Fact]
    public void CountWords_When2WordsWithMultipleSpaces_ThenReturns2()
    {
        var twoWordsWithMultipleSpaces = "Hey   guys";

        var wordCount = wordCounter.CountWords(twoWordsWithMultipleSpaces);

        wordCount.Should().Be(2);
    }

    [Fact]
    public void CountWords_When2WordsWithNewline_ThenReturns2()
    {
        var twoWordsWithNewline = "Hey\nguys";

        var wordCount = wordCounter.CountWords(twoWordsWithNewline);

        wordCount.Should().Be(2);
    }
}
