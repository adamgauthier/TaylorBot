using Discord;

namespace TaylorBot.Net.Commands.DiscordNet;

public static class DiscordNetContextMapper
{
    public static RunContext MapToRunContext(ITaylorBotCommandContext context)
    {
        var channelType = context.Channel.GetChannelType();
        ArgumentNullException.ThrowIfNull(channelType);

        RunContext runContext = new(
            context.Message.Timestamp,
            new(context.User),
            context.User,
            new(context.Channel.Id, channelType.Value),
            context.Guild != null ? new(context.Guild.Id, context.Guild) : null,
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
        var commandInfo = context.CommandInfos.OrderByDescending(c => c.Priority).FirstOrDefault();

        return new(commandInfo?.Aliases[0]!, commandInfo?.Module.Name, commandInfo?.Aliases?.ToList());
    }
}
