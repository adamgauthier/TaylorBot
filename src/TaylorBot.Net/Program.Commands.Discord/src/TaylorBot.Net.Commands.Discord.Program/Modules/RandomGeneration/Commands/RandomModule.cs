using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;

[Name("Random 🎲")]
public class RandomModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("dice")]
    public async Task<RuntimeResult> DiceAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: DiceSlashCommand.CommandName, IsRemoved: true));

    [Command("choose")]
    [Alias("choice")]
    public async Task<RuntimeResult> ChooseAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: ChooseSlashCommand.CommandName, IsRemoved: true));
}
