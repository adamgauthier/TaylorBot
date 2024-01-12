using FluentAssertions;
using TaylorBot.Net.Core.Embed;
using Xunit;

namespace TaylorBot.Net.Commands.Types.Tests;

public class StringEnumerableEmbedExtensionsTests
{
    [Fact]
    public void CreateEmbedDescriptionWithMaxAmountOfLines_WhenFirst3LinesHave4094Length_ThenReturnsJoinedFirst3Lines()
    {
        var line1 = CreateLine('1', 2000);
        var line2 = CreateLine('2', 2000);
        var line3 = CreateLine('3', 94);
        var line4 = CreateLine('4', 1);
        var line5 = CreateLine('5', 25);
        var line6 = CreateLine('6', 500);

        var result = new[] {
            line1,
            line2,
            line3,
            line4,
            line5,
            line6
        }.CreateEmbedDescriptionWithMaxAmountOfLines();

        result.Should().Be(string.Join('\n', new[] { line1, line2, line3 }));
    }

    private static string CreateLine(char character, int length)
    {
        return string.Join(string.Empty, Enumerable.Repeat(character, length));
    }
}
