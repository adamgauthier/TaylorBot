namespace TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;

public class SimpleCommandRunner : ICommandRunner
{
    public async Task<ICommandResult> RunInteractionAsync(Command command, RunContext context)
    {
        return await command.RunAsync();
    }

    public async ValueTask<ICommandResult> RunSlashCommandAsync(Command command, RunContext context)
    {
        return await command.RunAsync();
    }
}
