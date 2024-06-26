﻿using Discord;

namespace TaylorBot.Net.Core.User;

public static class UserExtensions
{
    public static string GetAvatarUrlOrDefault(this DiscordUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
    {
        return CDN.GetUserAvatarUrl(user.Id, user.Avatar, size, format) ??
            (user.Discriminator != "0"
                ? CDN.GetDefaultUserAvatarUrl(ushort.Parse(user.Discriminator))
                : CDN.GetDefaultUserAvatarUrl(user.Id));
    }

    public static string GetGuildAvatarUrlOrDefault(this DiscordUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
    {
        return user.MemberInfo?.GuildAvatar != null
            ? CDN.GetGuildUserAvatarUrl(user.Id, user.MemberInfo.GuildId, user.MemberInfo.GuildAvatar, size, format)
            : user.GetAvatarUrlOrDefault(format, size);
    }

    public static string GetAvatarUrlOrDefault(this IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
    {
        return CDN.GetUserAvatarUrl(user.Id, user.AvatarId, size, format) ??
            (user.DiscriminatorValue != 0
                ? CDN.GetDefaultUserAvatarUrl(user.DiscriminatorValue)
                : CDN.GetDefaultUserAvatarUrl(user.Id));
    }

    public static string GetGuildAvatarUrlOrDefault(this IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
    {
        return user is IGuildUser guildUser && guildUser.GuildAvatarId != null
            ? CDN.GetGuildUserAvatarUrl(guildUser.Id, guildUser.Guild.Id, guildUser.GuildAvatarId, size, format)
            : user.GetAvatarUrlOrDefault(format, size);
    }
}
