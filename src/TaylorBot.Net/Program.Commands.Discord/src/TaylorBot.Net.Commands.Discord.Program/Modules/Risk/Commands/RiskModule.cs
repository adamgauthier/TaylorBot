using Discord.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Risk.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

[Name("Risk 💰")]
public class RiskModule(ICommandRunner commandRunner, RiskPlaySlashCommand riskCommand) : TaylorBotModule
{
    [Command("gamble")]
    [Alias("supergamble", "sgamble")]
    [Summary("Get a group together and attempt to heist a taypoint bank")]
    public async Task<RuntimeResult> RiskAsync(
        [Summary("How much of your taypoints do you want to invest into heist equipment?")]
        [Remainder]
        string amount
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(
            riskCommand.Play(context, Context.User, amount: null, amountString: amount),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("gamblewins")]
    [Alias("gwins", "gambleprofits", "gprofits", "gamblefails", "gfails", "gamblelosses", "glosses")]
    [Summary("This command has been moved to **/risk profile**. Please use it instead! 😊")]
    public async Task<RuntimeResult> RiskWinsAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 **/risk profile** 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("rankgamblewins")]
    [Alias("rank gamblewins", "rank gwins", "rankgwins")]
    [Summary("This command has been moved to **/risk leaderboard**. Please use it instead! 😊")]
    public async Task<RuntimeResult> RankHeistWinsAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 **/risk leaderboard** 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
