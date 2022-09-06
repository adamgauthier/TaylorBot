using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.Strings
{
    public static class StringExtensions
    {
        public static string EscapeNewLines(this string toEscape)
        {
            return toEscape.Replace("\n", @"\n");
        }

        public static string DiscordMdLink(this string text, string url)
        {
            return $"[{text}]({url.Replace(")", "%29")})";
        }

        public static string MdUserLink(this string text, SnowflakeId userId)
        {
            return text.DiscordMdLink($"https://discordapp.com/users/{userId}");
        }
    }
}
