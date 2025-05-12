using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Commands;

[Name("Usernames 🏷️")]
[Group("usernames")]
[Alias("names")]
public class UsernamesModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Priority(-1)]
    [Command]
    [Summary("This command has been moved to 👉 </usernames show:1214813880463921242> 👈 Please use it instead! 😊")]
    public async Task<RuntimeResult> GetAsync(
        [Remainder]
        string? _ = null)
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </usernames show:1214813880463921242> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("private")]
    [Summary("This command has been moved to 👉 </usernames visibility:1214813880463921242> 👈 Please use it instead! 😊")]
    public async Task<RuntimeResult> PrivateAsync(
        [Remainder]
        string? _ = null)
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </usernames visibility:1214813880463921242> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("public")]
    [Summary("This command has been moved to 👉 </usernames visibility:1214813880463921242> 👈 Please use it instead! 😊")]
    public async Task<RuntimeResult> PublicAsync(
        [Remainder]
        string? _ = null)
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </usernames visibility:1214813880463921242> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
