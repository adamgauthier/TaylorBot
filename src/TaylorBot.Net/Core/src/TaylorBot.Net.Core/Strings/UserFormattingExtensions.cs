using Discord;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Core.Strings;

public static class UserFormattingExtensions
{
    public static string FormatTagAndMention(this DiscordUser user)
    {
        return $"{user.Handle} ({user.Mention})";
    }

    public static string FormatTagAndMention(this IUser user) => new DiscordUser(user).FormatTagAndMention();

    public static string Handle(this IUser user) => new DiscordUser(user).Handle;
}
