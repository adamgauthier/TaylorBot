using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Roll.Commands;

[Name("Points 💰")]
public class RollModule(ICommandRunner commandRunner, RollPlaySlashCommand playCommand) : TaylorBotModule
{
    [Command("roll")]
    [Summary("Roll the Taylor Machine for a chance to win taypoints")]
    public async Task<RuntimeResult> RollAsync(
        [Remainder]
        string? _ = null
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(
            playCommand.Play(context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("rolls")]
    [Alias("perfectrolls", "prolls", "1989rolls")]
    [Summary("This command has been moved to **/roll profile**. Please use it instead! 😊")]
    public async Task<RuntimeResult> RollsAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 **/roll profile** 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("rankrolls")]
    [Alias("rank rolls", "rankperfectrolls", "rank perfectrolls", "rankprolls", "rank prolls", "rank1989rolls", "rank 1989rolls")]
    [Summary("This command has been moved to **/roll leaderboard**. Please use it instead! 😊")]
    public async Task<RuntimeResult> RankRollsAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 **/roll leaderboard** 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
