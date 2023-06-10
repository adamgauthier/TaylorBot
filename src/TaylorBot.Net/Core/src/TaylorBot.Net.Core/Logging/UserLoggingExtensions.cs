using Discord;

namespace TaylorBot.Net.Core.Logging;

public static class UserLoggingExtensions
{
    public static string FormatLog(this IUser user)
    {
        return $"{user.Username}#{user.Discriminator} ({user.Id})";
    }
}
