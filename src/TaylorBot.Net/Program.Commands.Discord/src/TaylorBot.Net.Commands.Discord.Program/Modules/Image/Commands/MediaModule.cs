using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;

[Name("Media 📷")]
public class MediaModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("image")]
    [Alias("imagen")]
    public async Task<RuntimeResult> MediaAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: ImageSlashCommand.CommandName, IsRemoved: true));
}
