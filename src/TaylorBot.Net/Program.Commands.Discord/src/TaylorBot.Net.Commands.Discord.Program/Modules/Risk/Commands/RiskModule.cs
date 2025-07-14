using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Risk.Commands;

[Name("Risk 💰")]
public class RiskModule(ICommandRunner commandRunner, RiskPlaySlashCommand riskCommand, PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command(RiskPlaySlashCommand.PrefixCommandName)]
    public async Task<RuntimeResult> RiskAsync(
        [Remainder]
        string amount
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: RiskPlaySlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            riskCommand.Play(context, context.User, RiskLevel.Low, amount: null, amountString: amount),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command(RiskPlaySlashCommand.PrefixSuperCommandName)]
    [Alias(RiskPlaySlashCommand.PrefixSuperCommandAlias)]
    public async Task<RuntimeResult> SuperRiskAsync(
        [Remainder]
        string amount
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: RiskPlaySlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            riskCommand.Play(context, context.User, RiskLevel.High, amount: null, amountString: amount),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("gamblewins")]
    [Alias("gwins", "gambleprofits", "gprofits", "gamblefails", "gfails", "gamblelosses", "glosses")]
    public async Task<RuntimeResult> RiskWinsAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: RiskProfileSlashCommand.CommandName, IsRemoved: true));

    [Command("rankgamblewins")]
    [Alias("rank gamblewins", "rank gwins", "rankgwins")]
    public async Task<RuntimeResult> RankRiskWinsAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: RiskLeaderboardSlashCommand.CommandName, IsRemoved: true));
}
