using Discord;
using System;
using System.Linq;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Core.Embed
{
    public static class EmbedBuilderExtensions
    {
        public static EmbedBuilder WithUserAsAuthor(this EmbedBuilder embedBuilder, IUser user)
        {
            var author = $"{user.Username}#{user.Discriminator}";
            if (user.IsBot)
                author += " 🤖";

            var avatarUrl = user.GetAvatarUrlOrDefault();

            return embedBuilder.WithAuthor(author, iconUrl: avatarUrl, url: avatarUrl);
        }

        public static EmbedBuilder WithUserAsAuthorAndColor(this EmbedBuilder embedBuilder, IUser user)
        {
            return embedBuilder
                .WithUserAsAuthor(user)
                .WithColor(GetColorFromStatus(user.Status));
        }

        private static Color GetColorFromStatus(UserStatus userStatus)
        {
            switch (userStatus)
            {
                case UserStatus.Offline:
                case UserStatus.Invisible:
                    return new Color(116, 127, 141);

                case UserStatus.Online:
                    return new Color(67, 181, 129);

                case UserStatus.Idle:
                case UserStatus.AFK:
                    return new Color(250, 166, 26);

                case UserStatus.DoNotDisturb:
                    return new Color(240, 71, 71);

                default:
                    throw new ArgumentOutOfRangeException(nameof(userStatus));
            }
        }

        public static EmbedBuilder WithGuildAsAuthor(this EmbedBuilder embedBuilder, IGuild guild)
        {
            return embedBuilder
                .WithAuthor(guild.Features.Contains("VIP_REGIONS") ? $"{guild.Name} ⭐" : guild.Name, guild.IconUrl);
        }
    }
}
