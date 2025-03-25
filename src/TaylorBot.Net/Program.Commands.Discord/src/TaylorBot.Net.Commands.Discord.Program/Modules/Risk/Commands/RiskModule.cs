using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Risk.Commands;

[Name("Risk 💰")]
public class RiskModule(ICommandRunner commandRunner, RiskPlaySlashCommand riskCommand) : TaylorBotModule
{
    [Command(RiskPlaySlashCommand.PrefixCommandName)]
    [Summary("Risk some of your taypoints in an investment opportunity (50% chance to win)")]
    public async Task<RuntimeResult> RiskAsync(
        [Summary("How much of your taypoints do you want to invest into this risk?")]
        [Remainder]
        string amount
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            riskCommand.Play(context, context.User, RiskLevel.Low, amount: null, amountString: amount),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command(RiskPlaySlashCommand.PrefixSuperCommandName)]
    [Alias(RiskPlaySlashCommand.PrefixSuperCommandAlias)]
    [Summary("Risk some of your taypoints in an investment opportunity (10% chance to win)")]
    public async Task<RuntimeResult> SuperRiskAsync(
        [Summary("How much of your taypoints do you want to invest into this risk?")]
        [Remainder]
        string amount
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            riskCommand.Play(context, context.User, RiskLevel.High, amount: null, amountString: amount),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("gamblewins")]
    [Alias("gwins", "gambleprofits", "gprofits", "gamblefails", "gfails", "gamblelosses", "glosses")]
    [Summary("This command has been moved to </risk profile:1190786063136993431>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RiskWinsAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </risk profile:1190786063136993431> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("rankgamblewins")]
    [Alias("rank gamblewins", "rank gwins", "rankgwins")]
    [Summary("This command has been moved to </risk leaderboard:1190786063136993431>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RankHeistWinsAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </risk leaderboard:1190786063136993431> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
