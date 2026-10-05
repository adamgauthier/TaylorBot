using System.Text;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.Strings;

public static class StringExtensions
{
    public static string EscapeDiscordMarkdown(this string text)
    {
        StringBuilder escaped = new(text.Length);
        foreach (var character in text)
        {
            if (character is '\\' or '*' or '_' or '~' or '`' or '.' or ':' or '/'
                or '>' or '|' or '#' or '[' or '-' or '+' or '<')
            {
                escaped.Append('\\');
            }
            escaped.Append(character);
        }
        return escaped.ToString();
    }

    public static string EscapeNewLines(this string toEscape)
    {
        return toEscape.Replace("\n", @"\n", StringComparison.InvariantCulture);
    }

    public static string LinkToMessage(this string messageId, string channelId, string guildId)
    {
        return $"https://discord.com/channels/{guildId}/{channelId}/{messageId}";
    }

    public static string DiscordMdLink(this string text, string url)
    {
        return $"[{text}]({url.Replace(")", "%29", StringComparison.InvariantCulture)})";
    }

    public static string MdUserLink(this string text, SnowflakeId userId)
    {
        return text.DiscordMdLink($"https://discordapp.com/users/{userId}");
    }
}
