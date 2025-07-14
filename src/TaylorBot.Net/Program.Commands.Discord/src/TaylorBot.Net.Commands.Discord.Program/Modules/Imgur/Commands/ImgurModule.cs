using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Commands;

[Name("Media 📷")]
public class ImgurModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("imgur")]
    public async Task<RuntimeResult> ImgurAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: ImgurSlashCommand.CommandName, IsRemoved: true));
}
