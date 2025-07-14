using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Roll.Commands;

[Name("Points 💰")]
public class RollModule(ICommandRunner commandRunner, RollPlaySlashCommand playCommand, PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command(RollPlaySlashCommand.PrefixCommandName)]
    public async Task<RuntimeResult> RollAsync(
        [Remainder]
        string? _ = null
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: RollPlaySlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            playCommand.Play(context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("rolls")]
    [Alias("perfectrolls", "prolls", "1989rolls")]
    public async Task<RuntimeResult> RollsAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: RollProfileSlashCommand.CommandName, IsRemoved: true));

    [Command("rankrolls")]
    [Alias("rank rolls", "rankperfectrolls", "rank perfectrolls", "rankprolls", "rank prolls", "rank1989rolls", "rank 1989rolls")]
    public async Task<RuntimeResult> RankRollsAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: RollLeaderboardSlashCommand.CommandName, IsRemoved: true));
}
