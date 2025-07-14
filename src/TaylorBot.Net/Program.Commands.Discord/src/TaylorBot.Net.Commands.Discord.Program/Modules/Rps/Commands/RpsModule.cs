using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Rps.Commands;

[Name("Rps ✂️")]
public class RpsModule(ICommandRunner commandRunner, RpsPlaySlashCommand playCommand, PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command(RpsPlaySlashCommand.PrefixCommandName)]
    public async Task<RuntimeResult> RockPaperScissorsAsync(
        [Remainder]
        string? option = null
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: RpsPlaySlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            playCommand.Play(context, shape: null, shapeString: option),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("rpswins")]
    public async Task<RuntimeResult> RpsWinsAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: RpsProfileSlashCommand.CommandName, IsRemoved: true));

    [Command("rankrpswins")]
    [Alias("rank rpswins")]
    public async Task<RuntimeResult> RankRpsWinsAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: RpsLeaderboardSlashCommand.CommandName, IsRemoved: true));
}
