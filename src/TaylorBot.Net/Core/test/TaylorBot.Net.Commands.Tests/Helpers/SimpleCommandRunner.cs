namespace TaylorBot.Net.Commands.Tests.Helpers
{
    public class SimpleCommandRunner : ICommandRunner
    {
        public async ValueTask<ICommandResult> RunAsync(Command command, RunContext context)
        {
            return await command.RunAsync();
        }
    }
}
