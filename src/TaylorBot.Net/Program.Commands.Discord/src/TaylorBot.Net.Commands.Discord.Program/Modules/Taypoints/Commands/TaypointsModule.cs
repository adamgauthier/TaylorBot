using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

[Name("Taypoints 🪙")]
[Group("taypoints")]
[Alias("points")]
public class TaypointsModule : TaylorBotModule
{
    private readonly ICommandRunner _commandRunner;
    private readonly TaypointsBalanceCommand _taypointsBalanceCommand;

    public TaypointsModule(ICommandRunner commandRunner, TaypointsBalanceCommand taypointsBalanceCommand)
    {
        _commandRunner = commandRunner;
        _taypointsBalanceCommand = taypointsBalanceCommand;
    }

    [Command]
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
}

