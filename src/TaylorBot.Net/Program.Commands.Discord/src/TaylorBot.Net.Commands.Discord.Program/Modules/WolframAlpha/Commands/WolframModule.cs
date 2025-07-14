using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Commands;

[Name("Wolfram 🤖")]
public class WolframModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("wolframalpha")]
    [Alias("wolfram", "wa")]
    public async Task<RuntimeResult> WolframAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: WolframAlphaSlashCommand.CommandName, IsRemoved: true));
}
