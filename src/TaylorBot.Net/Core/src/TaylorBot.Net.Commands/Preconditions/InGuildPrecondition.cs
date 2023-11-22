using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Preconditions;

public class InGuildPrecondition : ICommandPrecondition
{
    public ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild == null)
        {
            return new(new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} can only be used in a guild",
                UserReason: new($"You can't use `{command.Metadata.Name}` because it can only be used in a server.")
            ));
        }

        return new(new PreconditionPassed());
    }
}
