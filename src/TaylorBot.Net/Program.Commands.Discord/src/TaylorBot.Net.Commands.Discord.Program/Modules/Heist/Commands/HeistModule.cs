using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Commands;

[Name("Heist 💰")]
public class HeistModule(ICommandRunner commandRunner, HeistPlaySlashCommand heistCommand, PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command(HeistPlaySlashCommand.PrefixCommandName)]
    public async Task<RuntimeResult> HeistAsync(
        [Remainder]
        string amount
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: HeistPlaySlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            heistCommand.Heist(context, amount: null, amountString: amount),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("heistwins")]
    [Alias("hwins", "heistprofits", "hprofits", "heistfails", "hfails", "heistlosses", "hlosses")]
    public async Task<RuntimeResult> HeistWinsAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: HeistProfileSlashCommand.CommandName, IsRemoved: true));

    [Command("rankheistwins")]
    [Alias("rank heistwins")]
    public async Task<RuntimeResult> RankHeistWinsAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: HeistLeaderboardSlashCommand.CommandName, IsRemoved: true));
}
