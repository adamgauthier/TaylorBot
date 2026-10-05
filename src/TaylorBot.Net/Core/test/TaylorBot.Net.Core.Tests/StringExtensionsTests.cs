using FluentAssertions;
using TaylorBot.Net.Core.Strings;
using Xunit;

namespace TaylorBot.Net.Core.Tests;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("Pizza, sushi & ramen", "Pizza, sushi & ramen")]
    [InlineData("**bold** _italic_ ~~strike~~ ||spoiler||", @"\*\*bold\*\* \_italic\_ \~\~strike\~\~ \|\|spoiler\|\|")]
    [InlineData("```code```", @"\`\`\`code\`\`\`")]
    [InlineData("# Heading\n> Quote\r\n- List", "\\# Heading\n\\> Quote\r\n\\- List")]
    [InlineData("-# Small\n+ List\n1. Ordered", "\\-\\# Small\n\\+ List\n1\\. Ordered")]
    [InlineData("[Dinner](https://example.com)", @"\[Dinner](https\:\/\/example\.com)")]
    [InlineData("<@123456789012345678>", @"\<@123456789012345678\>")]
    [InlineData(@"\**Dinner**", @"\\\*\*Dinner\*\*")]
    [InlineData("\uD83C\uDF55 Caf\u00E9", "\uD83C\uDF55 Caf\u00E9")]
    public void EscapeDiscordMarkdown_EscapesFormattingAndPreservesText(string text, string expected)
    {
        var escaped = text.EscapeDiscordMarkdown();

        escaped.Should().Be(expected);
    }
}
