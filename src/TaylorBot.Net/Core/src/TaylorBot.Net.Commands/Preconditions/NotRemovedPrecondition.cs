namespace TaylorBot.Net.Commands.Preconditions;

public class NotRemovedPrecondition(CommandMentioner mention) : ICommandPrecondition
{
    public ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.PrefixCommand?.IsRemoved == true)
        {
            var userReason = context.PrefixCommand.ReplacementSlashCommands != null ?
                context.PrefixCommand.ReplacementSlashCommands.Count > 1 ?
                    $"""
                    This command has been moved to:
                    {string.Join('\n', context.PrefixCommand.ReplacementSlashCommands.Select(c => $"👉 {mention.SlashCommand(c)} 👈"))}
                    Please use them instead! 😊
                    """ :
                    $"""
                    This command has been moved to 👉 {mention.SlashCommand(context.PrefixCommand.ReplacementSlashCommands[0])} 👈
                    Please use it instead! 😊
                    """ :
                    $"""
                    This command has been removed, sorry! 😕
                    {context.PrefixCommand.RemovedMessage}
                    """;

            return new(new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} is removed",
                UserReason: new(userReason)));
        }
        return new(new PreconditionPassed());
    }
}
