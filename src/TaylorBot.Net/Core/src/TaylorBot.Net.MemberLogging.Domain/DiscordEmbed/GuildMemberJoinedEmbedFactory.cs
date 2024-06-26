﻿using Discord;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.MemberLogging.Domain.Options;

namespace TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;

public class GuildMemberJoinedEmbedFactory(IOptionsMonitor<MemberLoggingOptions> optionsMonitor)
{
    private static EmbedBuilder CreateBaseEmbed(IGuildUser guildUser)
    {
        var avatarUrl = guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl();
        return new EmbedBuilder()
            .WithAuthor($"{guildUser.Handle()} ({guildUser.Id})", avatarUrl, avatarUrl)
            .WithTimestamp(guildUser.JoinedAt ?? DateTimeOffset.Now);
    }

    public Embed CreateMemberFirstJoined(IGuildUser guildUser)
    {
        return CreateBaseEmbed(guildUser)
            .WithColor(DiscordColor.FromHexString(optionsMonitor.CurrentValue.FirstJoinedEmbedColor))
            .WithFooter("User joined")
            .Build();
    }

    public Embed CreateMemberRejoined(IGuildUser guildUser, DateTimeOffset firstJoinedAt)
    {
        return CreateBaseEmbed(guildUser)
            .WithColor(DiscordColor.FromHexString(optionsMonitor.CurrentValue.RejoinedEmbedColor))
            .WithDescription($"`❕` {guildUser.Mention} first joined on {firstJoinedAt.FormatFullUserDate(TaylorBotCulture.Culture)}.")
            .WithFooter("User rejoined")
            .Build();
    }
}
