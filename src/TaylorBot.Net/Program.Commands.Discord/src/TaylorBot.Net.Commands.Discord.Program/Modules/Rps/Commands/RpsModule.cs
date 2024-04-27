using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Rps.Commands;

[Name("Rps ✂️")]
public class RpsModule(ICommandRunner commandRunner, RpsPlaySlashCommand playCommand) : TaylorBotModule
{
    [Command(RpsPlaySlashCommand.PrefixCommandName)]
    [Summary("Play a game of rock paper scissors with the bot. If you win, you gain 1 taypoint.")]
    public async Task<RuntimeResult> RockPaperScissorsAsync(
        [Summary("What move do you want to pick (rock, paper or scissors)?")]
        [Remainder]
        string? option = null
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(
            playCommand.Play(context, shape: null, shapeString: option),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("rpswins")]
    [Summary("This command has been moved to </rps profile:1185806478435680387>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RpsWinsAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </rps profile:1185806478435680387> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("rankrpswins")]
    [Alias("rank rpswins")]
    [Summary("This command has been moved to </rps leaderboard:1185806478435680387>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RankRpsWinsAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </rps leaderboard:1185806478435680387> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
