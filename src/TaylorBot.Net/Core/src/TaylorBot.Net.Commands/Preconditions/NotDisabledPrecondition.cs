namespace TaylorBot.Net.Commands.Preconditions;

public interface IDisabledCommandRepository
{
    ValueTask<string> InsertOrGetCommandDisabledMessageAsync(CommandMetadata command);
    ValueTask EnableGloballyAsync(string commandName);
    ValueTask<string> DisableGloballyAsync(string commandName, string disabledMessage);
}

public class NotDisabledPrecondition(IDisabledCommandRepository disabledCommandRepository, CommandMentioner mention) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        var disabledMessage = await disabledCommandRepository.InsertOrGetCommandDisabledMessageAsync(command.Metadata);

        return !string.IsNullOrEmpty(disabledMessage) ?
            new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} is globally disabled",
                UserReason: new(
                    $"""
                    You can't use {mention.Command(command, context)} because it is globally disabled right now 😕
                    {disabledMessage}
                    """)
            ) :
            new PreconditionPassed();
    }
}
