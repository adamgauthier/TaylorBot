using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Helpers
{
    public class SimpleCommandRunner : ICommandRunner
    {
        public async ValueTask<ICommandResult> RunAsync(Command command, RunContext context, IList<ICommandPrecondition>? additionalPreconditions = null)
        {
            return await command.RunAsync();
        }
    }
}
