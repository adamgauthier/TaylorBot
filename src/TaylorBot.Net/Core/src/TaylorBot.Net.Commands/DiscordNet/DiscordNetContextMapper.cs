using System.Linq;

namespace TaylorBot.Net.Commands.DiscordNet
{
    public static class DiscordNetContextMapper
    {
        public static RunContext MapToRunContext(ITaylorBotCommandContext context) =>
            new(context.Message.Timestamp, context.User, context.Channel, context.Guild, context.Client, context.CommandPrefix, new());
        public static CommandMetadata MapToCommandMetadata(ITaylorBotCommandContext context)
        {
            var commandInfo = context.CommandInfos.OrderByDescending(c => c.Priority).FirstOrDefault();

            return new(commandInfo?.Aliases[0]!, commandInfo?.Module.Name, commandInfo?.Aliases);
        }
    }
}
