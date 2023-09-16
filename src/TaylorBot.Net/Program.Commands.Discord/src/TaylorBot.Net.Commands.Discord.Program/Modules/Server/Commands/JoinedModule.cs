using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

[Name("Joined 🚪")]
public class JoinedModule : TaylorBotModule
{
    private readonly ICommandRunner _commandRunner;
    private readonly ServerJoinedSlashCommand _serverJoinedCommand;

    public JoinedModule(ICommandRunner commandRunner, ServerJoinedSlashCommand serverJoinedCommand)
    {
        _commandRunner = commandRunner;
        _serverJoinedCommand = serverJoinedCommand;
    }

    [Command("joined")]
    [Summary("Show the first recorded joined date of a server member")]
    public async Task<RuntimeResult> JoinedAsync(
        [Summary("What user would you like to see the joined date of?")]
        [Remainder]
        IUserArgument<IGuildUser>? user = null
    )
    {
        var u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(
            _serverJoinedCommand.Joined(u, isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("rankjoined")]
    [Alias("rank joined")]
    [Summary("This command has been moved to </server timeline:1137547317549998130>. Please use it instead! 😊")]
    public async Task<RuntimeResult> SetAgeAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </server timeline:1137547317549998130> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
