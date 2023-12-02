namespace TaylorBot.Net.Commands.Preconditions;

public interface IDisabledCommandRepository
{
    ValueTask<string> InsertOrGetCommandDisabledMessageAsync(CommandMetadata command);
    ValueTask EnableGloballyAsync(string commandName);
    ValueTask<string> DisableGloballyAsync(string commandName, string disabledMessage);
}

public class NotDisabledPrecondition : ICommandPrecondition
{
    private readonly IDisabledCommandRepository _disabledCommandRepository;

    public NotDisabledPrecondition(IDisabledCommandRepository disabledCommandRepository)
    {
        _disabledCommandRepository = disabledCommandRepository;
    }

    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        var disabledMessage = await _disabledCommandRepository.InsertOrGetCommandDisabledMessageAsync(command.Metadata);

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
