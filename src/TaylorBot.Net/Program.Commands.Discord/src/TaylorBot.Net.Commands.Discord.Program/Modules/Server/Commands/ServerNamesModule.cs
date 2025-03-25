using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

[Name("Stats 📊")]
public class ServerNamesModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Command("servernames")]
    [Alias("snames", "guildnames", "gnames")]
    [Summary("This command has been moved to </server names:1137547317549998130>. Please use it instead! 😊")]
    public async Task<RuntimeResult> ServerNamesAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </server names:1137547317549998130> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("profile")]
    [Alias("info", "asl")]
    [Summary("This command has been moved to </birthday age:1016938623880400907>, </location show:1141925890448691270>, </gender show:1150180971224764510>. Please use it instead! 😊")]
    public async Task<RuntimeResult> ProfileAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to:
                👉 </birthday age:1016938623880400907> 👈
                👉 </location show:1141925890448691270> 👈
                👉 </gender show:1150180971224764510> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
