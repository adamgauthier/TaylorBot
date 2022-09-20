using Discord;

namespace TaylorBot.Net.Core.User
{
    public static class UserExtensions
    {
        public static string GetAvatarUrlOrDefault(this IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            return user.GetAvatarUrl(format, size) ?? user.GetDefaultAvatarUrl();
        }

        public static string GetGuildAvatarUrlOrDefault(this IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            return user is IGuildUser guildUser ?
                guildUser.GuildAvatarId != null ? 
                    guildUser.GetGuildAvatarUrl(format, size) :
                    user.GetAvatarUrlOrDefault(format, size)
                : user.GetAvatarUrlOrDefault(format, size);
        }
    }
}
