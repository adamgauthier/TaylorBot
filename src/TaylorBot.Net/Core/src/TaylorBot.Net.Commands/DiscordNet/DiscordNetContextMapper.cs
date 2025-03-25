using Discord;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.DiscordNet;

public static class DiscordNetContextMapper
{
    public static RunContext MapToRunContext(ITaylorBotCommandContext context)
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
            new(string.Empty, string.Empty),
            new(Task.FromResult(context.CommandPrefix)),
            new(),
            context.Activity.Value
        );
        context.RunContext = runContext;
        return runContext;
    }

    public static CommandMetadata MapToCommandMetadata(ITaylorBotCommandContext context)
    {
        if (context.IsTestEnv)
        {
            return new CommandMetadata(null!, IsSlashCommand: false);
        }

        var commandInfo = context.CommandInfos.OrderByDescending(c => c.Priority).First();

        return new(commandInfo.Aliases[0], commandInfo.Module.Name, commandInfo.Aliases, IsSlashCommand: false);
    }
}
