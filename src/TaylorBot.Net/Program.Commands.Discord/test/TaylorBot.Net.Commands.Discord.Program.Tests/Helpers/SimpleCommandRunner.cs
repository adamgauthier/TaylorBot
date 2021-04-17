using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Helpers
{
    public class SimpleCommandRunner : ICommandRunner
    {
        public async ValueTask<ICommandResult> RunAsync(Command command, RunContext context)
        {
            return await command.RunAsync();
        }
    }
}
