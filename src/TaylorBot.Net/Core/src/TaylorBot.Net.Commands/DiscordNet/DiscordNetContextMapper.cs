namespace TaylorBot.Net.Commands.DiscordNet;

public static class DiscordNetContextMapper
{
    public static RunContext MapToRunContext(ITaylorBotCommandContext context)
    {
        RunContext runContext = new(
            context.Message.Timestamp,
            new(context.User),
            context.User,
            new(context.Channel.Id),
            context.Guild != null ? new(context.Guild.Id, context.Guild) : null,
            context.Client,
            context.CurrentUser,
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
