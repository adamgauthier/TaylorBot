using Discord;

namespace TaylorBot.Net.Core.Strings;

public static class UserFormattingExtensions
{
    public static string FormatTagAndMention(this IUser user)
    {
        return $"{user.Username}{user.DiscrimSuffix()} ({user.Mention})";
    }

    public static string DiscrimSuffix(this IUser user)
    {
        if (user.Discriminator == "0" || user.Discriminator == "0000")
        {
            return "";
        }
        else
        {
            return $"#{user.Discriminator}";
        }
    }
}
