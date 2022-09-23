﻿using Discord;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Core.Embed
{
    public static class EmbedBuilderExtensions
    {
        public static EmbedBuilder WithUserAsAuthor(this EmbedBuilder embedBuilder, IUser user, bool? showGuildAvatar = true)
        {
            var author = $"{user.Username}#{user.Discriminator}";
            if (user.IsBot)
                author += " 🤖";

            var avatarUrl = showGuildAvatar == true ? 
                user.GetGuildAvatarUrlOrDefault() :
                user.GetAvatarUrlOrDefault();

            return embedBuilder.WithAuthor(author, iconUrl: avatarUrl, url: avatarUrl);
        }

        public static EmbedBuilder WithGuildAsAuthor(this EmbedBuilder embedBuilder, IGuild guild)
        {
            return embedBuilder
                .WithAuthor(guild.Features?.HasFeature(GuildFeature.Partnered) == true ? $"{guild.Name} ⭐" : guild.Name, guild.IconUrl);
        }
    }
}
