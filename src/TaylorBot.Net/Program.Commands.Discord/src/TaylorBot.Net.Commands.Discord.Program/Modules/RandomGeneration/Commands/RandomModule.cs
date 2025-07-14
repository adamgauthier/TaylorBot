using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;

[Name("Random 🎲")]
public class RandomModule(ICommandRunner commandRunner, PrefixedCommandRunner prefixedCommandRunner, ChooseSlashCommand chooseSlashCommand) : TaylorBotModule
{
    [Command("dice")]
    public async Task<RuntimeResult> DiceAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: DiceSlashCommand.CommandName, IsRemoved: true));

    [Command("choose")]
    [Alias("choice")]
    public async Task<RuntimeResult> ChooseAsync(
        [Remainder]
        string options
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: ChooseSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            chooseSlashCommand.Choose(options, context),
            context
        );

        return new TaylorBotResult(result, context);
    }
}
