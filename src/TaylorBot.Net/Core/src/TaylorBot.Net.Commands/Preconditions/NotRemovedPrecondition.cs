namespace TaylorBot.Net.Commands.Preconditions;

public class NotRemovedPrecondition(CommandMentioner mention) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.PrefixCommand?.IsRemoved == true)
        {
            FormattableString userReason = context.PrefixCommand.ReplacementSlashCommands switch
            {
                { Count: > 1 } replacements =>
                    $"""
                    This command has been moved to:
                    {mention.Join("\n", replacements.Select(c => (FormattableString)$"👉 {mention.Slash(c)} 👈"))}
                    Please use them instead! 😊
                    """,
                { } replacements =>
                    $"""
                    This command has been moved to 👉 {mention.Slash(replacements[0])} 👈
                    Please use it instead! 😊
                    """,
                null =>
                    $"""
                    This command has been removed, sorry! 😕
                    {context.PrefixCommand.RemovedMessage}
                    """,
            };

            return new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} is removed",
                UserReason: new(await mention.FormatAsync(context, userReason)));
        }
        return new PreconditionPassed();
    }
}
