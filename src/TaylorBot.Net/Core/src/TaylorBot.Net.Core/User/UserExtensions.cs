using Discord;

namespace TaylorBot.Net.Core.User
{
    public static class UserExtensions
    {
        public static string GetAvatarUrlOrDefault(this IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            return user.GetAvatarUrl(format, size) ?? user.GetDefaultAvatarUrl();
        }
    }
}
