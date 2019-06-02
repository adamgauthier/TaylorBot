using Discord;
using Humanizer;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using TaylorBot.Net.ChannelLogging.Domain.Options;

namespace TaylorBot.Net.ChannelLogging.Domain.DiscordEmbed
{
    public class GuildMemberJoinedEmbedFactory
    {
        private static readonly Color FirstJoinedColor = new Color(116, 214, 0);
        private static readonly Color RejoinedColor = new Color(0, 156, 26);

        private static readonly CultureInfo USCultureInfo = new CultureInfo("en-US");

        private readonly IOptionsMonitor<ChannelLoggingOptions> optionsMonitor;

        public GuildMemberJoinedEmbedFactory(IOptionsMonitor<ChannelLoggingOptions> optionsMonitor)
        {
            this.optionsMonitor = optionsMonitor;
        }

        private EmbedBuilder CreateBaseEmbed(IGuildUser guildUser)
        {
            var avatarUrl = guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl();

            return new EmbedBuilder()
                .WithAuthor($"{guildUser.Username}#{guildUser.Discriminator} ({guildUser.Id})", avatarUrl, avatarUrl)
                .WithTimestamp(guildUser.JoinedAt ?? DateTimeOffset.Now);
        }

        public Embed CreateMemberFirstJoined(IGuildUser guildUser)
        {
            return CreateBaseEmbed(guildUser)
                .WithColor(FirstJoinedColor)
                .WithFooter("User joined")
                .Build();
        }

        public Embed CreateMemberRejoined(IGuildUser guildUser, DateTimeOffset firstJoinedAt)
        {
            var options = optionsMonitor.CurrentValue;
            var utcFirstJoinedAt = firstJoinedAt.UtcDateTime;

            return CreateBaseEmbed(guildUser)
                .WithColor(RejoinedColor)
                .WithDescription($"`❕` {guildUser.Mention} first joined on {utcFirstJoinedAt.ToString(options.FirstJoinedAtUTCFormat)} ({firstJoinedAt.Humanize(culture: USCultureInfo)}).")
                .WithFooter("User rejoined")
                .Build();
        }
    }
}
