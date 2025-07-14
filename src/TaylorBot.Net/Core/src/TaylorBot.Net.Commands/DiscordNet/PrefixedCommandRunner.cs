using static TaylorBot.Net.Commands.RunContext;

namespace TaylorBot.Net.Commands.DiscordNet;

public class PrefixedCommandRunner(ICommandRunner commandRunner)
{
    public async Task<TaylorBotResult> RunAsync(
        ITaylorBotCommandContext context,
        PrefixCommandInfo commandInfo)
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(context),
            () => throw new InvalidOperationException());
        var runContext = DiscordNetContextMapper.MapToRunContext(context, commandInfo);

        var result = await commandRunner.RunSlashCommandAsync(command, runContext);

        return new TaylorBotResult(result, runContext);
    }
}
