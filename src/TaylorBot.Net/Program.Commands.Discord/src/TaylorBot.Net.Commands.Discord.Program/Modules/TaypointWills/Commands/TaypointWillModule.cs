using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Commands;

[Name("TaypointWill")]
[Group("taypointwill")]
public class TaypointWillModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Priority(-1)]
    [Command]
    [Summary("This command has been moved to </taypoints succession:1103846727880028180>")]
    public async Task<RuntimeResult> GetAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </taypoints succession:1103846727880028180> 👈
                Please use it instead! 😊
                """
            )))
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("add")]
    [Summary("This command has been moved to </taypoints succession:1103846727880028180>")]
    public async Task<RuntimeResult> AddAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </taypoints succession:1103846727880028180> 👈
                Please use it instead! 😊
                """
            )))
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("clear")]
    [Summary("This command has been moved to </taypoints succession:1103846727880028180>")]
    public async Task<RuntimeResult> ClearAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </taypoints succession:1103846727880028180> 👈
                Please use it instead! 😊
                """
            )))
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("claim")]
    [Summary("This command has been moved to </taypoints succession:1103846727880028180>")]
    public async Task<RuntimeResult> ClaimAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </taypoints succession:1103846727880028180> 👈
                Please use it instead! 😊
                """
            )))
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
