using System.Threading.Tasks;

namespace TaylorBot.Net.Commands
{
    public interface ICommandPrecondition
    {
        public ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context);
    }

    public record PreconditionPassed() : ICommandResult;
    public record PreconditionFailed(string PrivateReason, UserReason UserReason) : ICommandResult;
    public record UserReason(string Reason, bool HideInPrefixCommands = false);
}
