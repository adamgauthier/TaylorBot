using Discord;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.MemberLogging.Domain.Options;

namespace TaylorBot.Net.MemberLogging.Domain.DiscordEmbed
{
    public class GuildMemberBanEmbedFactory
    {
        private readonly IOptionsMonitor<MemberBanLoggingOptions> optionsMonitor;

        public GuildMemberBanEmbedFactory(IOptionsMonitor<MemberBanLoggingOptions> optionsMonitor)
        {
            this.optionsMonitor = optionsMonitor;
        }

        public Embed CreateMemberBanned(IUser user)
        {
            var options = optionsMonitor.CurrentValue;
            var avatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();

            return new EmbedBuilder()
                .WithAuthor($"{user.Username}#{user.Discriminator} ({user.Id})", avatarUrl, avatarUrl)
                .WithCurrentTimestamp()
                .WithColor(DiscordColor.FromHexString(options.MemberBannedEmbedColorHex))
                .WithFooter("User banned")
                .Build();
        }
    }
}
