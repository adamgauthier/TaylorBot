namespace TaylorBot.Net.Commands.DiscordNet;

public static class DiscordNetContextMapper
{
    public static RunContext MapToRunContext(ITaylorBotCommandContext context)
    {
        RunContext runContext = new(
            context.Message.Timestamp,
            context.User,
            new(context.Channel.Id.ToString()),
            context.Guild,
            context.Client,
            context.CurrentUser,
            new(string.Empty, string.Empty),
            context.CommandPrefix,
            new()
        );
        context.RunContext = runContext;
        return runContext;
    }

    public static CommandMetadata MapToCommandMetadata(ITaylorBotCommandContext context)
    {
        var commandInfo = context.CommandInfos.OrderByDescending(c => c.Priority).FirstOrDefault();

        return new(commandInfo?.Aliases[0]!, commandInfo?.Module.Name, commandInfo?.Aliases);
    }
}
