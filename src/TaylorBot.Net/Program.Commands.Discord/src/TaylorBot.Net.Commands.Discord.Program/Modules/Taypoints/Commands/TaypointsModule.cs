using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

[Name("Taypoints 🪙")]
public class TaypointsModule : TaylorBotModule
{
    private readonly ICommandRunner _commandRunner;
    private readonly TaypointsBalanceSlashCommand _taypointsBalanceCommand;

    public TaypointsModule(ICommandRunner commandRunner, TaypointsBalanceSlashCommand taypointsBalanceCommand)
    {
        _commandRunner = commandRunner;
        _taypointsBalanceCommand = taypointsBalanceCommand;
    }

    [Command("taypoints")]
    [Alias("points")]
    [Summary("Show the current taypoint balance of a user")]
    public async Task<RuntimeResult> BalanceAsync(
        [Summary("What user would you like to see the balance of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(
            _taypointsBalanceCommand.Balance(u, isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("ranktaypoints")]
    [Alias("rank taypoints", "rankpoints", "rank points")]
    [Summary("This command has been moved to </taypoints leaderboard:1103846727880028180>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RankTaypointsAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </taypoints leaderboard:1103846727880028180> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
