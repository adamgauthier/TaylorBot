using Discord;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.MemberLogging.Domain.Options;

namespace TaylorBot.Net.MemberLogging.Domain.DiscordEmbed
{
    public class GuildMemberLeftEmbedFactory
    {
        private readonly IOptionsMonitor<MemberLeftLoggingOptions> optionsMonitor;

        public GuildMemberLeftEmbedFactory(IOptionsMonitor<MemberLeftLoggingOptions> optionsMonitor)
        {
            this.optionsMonitor = optionsMonitor;
        }

        public Embed CreateMemberLeft(IGuildUser guildUser)
        {
            var options = optionsMonitor.CurrentValue;
            var avatarUrl = guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl();

            return new EmbedBuilder()
                .WithAuthor($"{guildUser.Username}#{guildUser.Discriminator} ({guildUser.Id})", avatarUrl, avatarUrl)
                .WithCurrentTimestamp()
                .WithColor(DiscordColor.FromHexString(options.MemberLeftEmbedColorHex))
                .WithFooter("User left")
                .Build();
        }
    }
}
