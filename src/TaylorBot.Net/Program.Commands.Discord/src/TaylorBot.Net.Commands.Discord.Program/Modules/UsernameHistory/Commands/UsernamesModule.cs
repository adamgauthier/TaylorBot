using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Commands;

[Name("Usernames 🏷️")]
[Group("usernames")]
[Alias("names")]
public class UsernamesModule(ICommandRunner commandRunner, UsernamesShowSlashCommand usernamesShowCommand) : TaylorBotModule
{
    [Priority(-1)]
    [Command]
    [Summary("Gets a list of previous usernames for a user.")]
    public async Task<RuntimeResult> GetAsync(
        [Summary("What user would you like to see the username history of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ? Context.User : await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(
            usernamesShowCommand.Show(u),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("private")]
    [Summary("This command has been moved to 👉 **/usernames visibility** 👈 Please use it instead! 😊")]
    public async Task<RuntimeResult> PrivateAsync()
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 **/usernames visibility** 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("public")]
    [Summary("This command has been moved to 👉 **/usernames visibility** 👈 Please use it instead! 😊")]
    public async Task<RuntimeResult> PublicAsync()
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 **/usernames visibility** 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
