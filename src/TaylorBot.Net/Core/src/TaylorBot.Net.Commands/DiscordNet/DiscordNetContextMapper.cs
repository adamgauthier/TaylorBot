using Discord;
using TaylorBot.Net.Core.Channel;
using static TaylorBot.Net.Commands.RunContext;

namespace TaylorBot.Net.Commands.DiscordNet;

public static class DiscordNetContextMapper
{
    public static RunContext MapToRunContext(ITaylorBotCommandContext context, PrefixCommandInfo? prefixInfo)
    {
        var channelType = context.Channel.GetChannelType();
        if (!channelType.HasValue)
        {
            throw new ArgumentNullException(nameof(channelType));
        }

        DiscordChannel channel = new(context.Channel.Id, channelType.Value);

        CommandGuild? guild = context.Guild != null ? new(context.Guild.Id, context.Guild) : null;

        RunContext runContext = new(
            context.Message.Timestamp,
            new(context.User),
            context.User,
            channel,
            guild,
            GuildTextChannel: guild != null ? new(channel.Id, guild.Id, channel.Type) : null,
            context.Client,
            SlashCommand: null,
            OnGoing: new(),
            context.Activity.Value,
            PrefixCommand: prefixInfo ?? new()
        );
        context.RunContext = runContext;
        return runContext;
    }

    public static CommandMetadata MapToCommandMetadata(ITaylorBotCommandContext context)
    {
        if (context.IsTestEnv)
        {
            return new(null!, IsSlashCommand: false);
        }

        var commandInfo = context.CommandInfos.OrderByDescending(c => c.Priority).First();

        return new(commandInfo.Aliases[0], commandInfo.Aliases, IsSlashCommand: false);
    }
}
