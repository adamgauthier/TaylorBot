using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;

[Name("Gender")]
public class GenderModule(ICommandRunner commandRunner, GenderShowSlashCommand genderShowCommand) : TaylorBotModule
{
    [Command(GenderShowSlashCommand.PrefixCommandName)]
    [Summary("Show the gender of a user")]
    public async Task<RuntimeResult> ShowGenderAsync(
        [Summary("What user would you like to see the gender of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ? Context.User : await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            genderShowCommand.Show(new(u)),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("setgender")]
    [Alias("set gender")]
    [Summary("This command has been moved to </gender set:1150180971224764510>. Please use it instead! 😊")]
    public async Task<RuntimeResult> SetGenderAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </gender set:1150180971224764510> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("cleargender")]
    [Alias("clear gender")]
    [Summary("This command has been moved to </gender clear:1150180971224764510>. Please use it instead! 😊")]
    public async Task<RuntimeResult> ClearGenderAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </gender clear:1150180971224764510> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
