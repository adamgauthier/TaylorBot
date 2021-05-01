using Discord;

namespace TaylorBot.Net.Core.Strings
{
    public static class UserFormattingExtensions
    {
        public static string FormatTagAndMention(this IUser user)
        {
            return $"{user.Username}#{user.Discriminator} ({user.Mention})";
        }
    }
}
