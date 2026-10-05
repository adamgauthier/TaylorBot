using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Commands;

[Name("YouTube")]
public class YouTubeModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("youtube")]
    [Alias("yt")]
    public async Task<RuntimeResult> SearchAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: YouTubeSlashCommand.CommandName, IsRemoved: true));
}
