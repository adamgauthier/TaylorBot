namespace TaylorBot.Net.Commands.Preconditions;

public interface IDisabledCommandRepository
{
    ValueTask<string> InsertOrGetCommandDisabledMessageAsync(CommandMetadata command);
    ValueTask EnableGloballyAsync(string commandName);
    ValueTask<string> DisableGloballyAsync(string commandName, string disabledMessage);
}

public class NotDisabledPrecondition(IDisabledCommandRepository disabledCommandRepository) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        var disabledMessage = await disabledCommandRepository.InsertOrGetCommandDisabledMessageAsync(command.Metadata);

        return disabledMessage != string.Empty ?
            new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} is globally disabled",
                UserReason: new(string.Join('\n', new[] {
                    $"You can't use `{command.Metadata.Name}` because it is globally disabled right now.",
                    disabledMessage
                }))
            ) :
            new PreconditionPassed();
    }
}
