﻿using Discord;
using FakeItEasy;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Channel;
using TaylorBot.Net.Core.User;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;

public enum ContextType { Guild, DM }

public static class CommandUtils
{
    public static readonly DiscordUser AUser = new(119341483219353602, "adam", "ecb368bd7eb540754c0bf5a2ce65af62", "0", IsBot: true, MemberInfo: null);

    public static IRateLimiter UnlimitedRateLimiter
    {
        get
        {
            var rateLimiter = A.Fake<IRateLimiter>(o => o.Strict());
            A.CallTo(() => rateLimiter.VerifyDailyLimitAsync(A<DiscordUser>.Ignored, A<string>.Ignored)).Returns(null);
            return rateLimiter;
        }
    }

    public static CommandMentioner Mentioner
    {
        get
        {
            var repository = A.Fake<IApplicationCommandsRepository>(o => o.Strict());
            A.CallTo(() => repository.GetCommandId(A<string>.Ignored)).Returns(null);
            return new(repository);
        }
    }

    public static RunContext CreateTestContext(ISlashCommand? command = null, ContextType contextType = ContextType.Guild)
    {
        CommandGuild? guild = contextType == ContextType.Guild
            ? new(167845806479638529, A.Fake<IGuild>())
            : null;

        DiscordChannel channel = new(167845806479638529, contextType == ContextType.Guild ? ChannelType.Text : ChannelType.DM);

        return new RunContext(
            DateTimeOffset.UtcNow,
            AUser,
            null,
            channel,
            guild,
            guild != null ? new GuildTextChannel(channel.Id, guild.Id, channel.Type) : null,
            null!,
            command is not null ? new("922354806574678086", command.Info.Name) : null,
            null!,
            null!);
    }
}
