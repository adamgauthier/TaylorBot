using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;

[Name("Gender")]
public class GenderModule : TaylorBotModule
{
    private readonly ICommandRunner _commandRunner;
    private readonly GenderShowCommand _genderShowCommand;

    public GenderModule(ICommandRunner commandRunner, GenderShowCommand genderShowCommand)
    {
        _commandRunner = commandRunner;
        _genderShowCommand = genderShowCommand;
    }

    [Command("gender")]
    [Summary("Show the gender of a user")]
    public async Task<RuntimeResult> HoroscopeAsync(
        [Summary("What user would you like to see the gender of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ? Context.User : await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(
            _genderShowCommand.Show(u),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("setgender")]
    [Alias("set gender")]
    [Summary("This command has been moved to **/gender set**. Please use instead! 😊")]
    public async Task<RuntimeResult> SetGenderAsync(
        [Remainder]
        string? text = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 **/gender set** 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("cleargender")]
    [Alias("clear gender")]
    [Summary("This command has been moved to **/gender clear**. Please use instead! 😊")]
    public async Task<RuntimeResult> ClearGenderAsync(
        [Remainder]
        string? text = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 **/gender clear** 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
