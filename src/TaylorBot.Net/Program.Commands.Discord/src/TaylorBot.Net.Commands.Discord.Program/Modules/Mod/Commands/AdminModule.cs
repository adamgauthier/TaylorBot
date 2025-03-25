using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Commands;

[Name("Admin 📜")]
public class AdminModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Command("addlogchannel")]
    [Alias("alc")]
    [Summary("This command has been moved to </monitor members set:887146682146488390> and </monitor deleted set:887146682146488390>. Please use it instead! 😊")]
    public async Task<RuntimeResult> AddLogAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to:
                👉 </monitor members set:887146682146488390> 👈
                👉 </monitor deleted set:887146682146488390> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("removelogchannel")]
    [Alias("rlc")]
    [Summary("This command has been moved to </monitor members stop:887146682146488390> and </monitor deleted stop:887146682146488390>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RemoveLogAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to:
                👉 </monitor members stop:887146682146488390> 👈
                👉 </monitor deleted stop:887146682146488390> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("addspamchannel")]
    [Alias("asc")]
    [Summary("This command has been moved to </mod spam add:838266590294048778>. Please use it instead! 😊")]
    public async Task<RuntimeResult> AddSpamAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </mod spam add:838266590294048778> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("removespamchannel")]
    [Alias("rsc")]
    [Summary("This command has been moved to </mod spam remove:838266590294048778>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RemoveSpamAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </mod spam remove:838266590294048778> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("disablechannelcommand")]
    [Alias("dcc")]
    [Summary("This command has been moved to </command channel-disable:909694280703016991>. Please use it instead! 😊")]
    public async Task<RuntimeResult> DisableChannelCommandAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </command channel-disable:909694280703016991> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("enablechannelcommand")]
    [Alias("ecc")]
    [Summary("This command has been moved to </command channel-enable:909694280703016991>. Please use it instead! 😊")]
    public async Task<RuntimeResult> EnableChannelCommandAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </command channel-enable:909694280703016991> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("disableservercommand")]
    [Alias("disableguildcommand", "dgc", "dsc")]
    [Summary("This command has been moved to </command server-disable:909694280703016991>. Please use it instead! 😊")]
    public async Task<RuntimeResult> DisableServerCommandAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </command server-disable:909694280703016991> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("enableservercommand")]
    [Alias("enableguildcommand", "egc", "esc")]
    [Summary("This command has been moved to </command server-enable:909694280703016991>. Please use it instead! 😊")]
    public async Task<RuntimeResult> EnableServerCommandAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </command server-enable:909694280703016991> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("addaccessiblerole")]
    [Alias("aar")]
    [Summary("This command has been moved to `roles add`. Please use it instead! 😊")]
    public async Task<RuntimeResult> AddRoleAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 `roles add` 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("removeaccessiblerole")]
    [Alias("rar")]
    [Summary("This command has been moved to `roles remove`. Please use it instead! 😊")]
    public async Task<RuntimeResult> RemoveRoleAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 `roles remove` 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("droprole")]
    [Alias("dr")]
    [Summary("This command has been moved to `role drop`. Please use it instead! 😊")]
    public async Task<RuntimeResult> DropRoleAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 `role drop` 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("getrole")]
    [Summary("This command has been moved to `role`. Please use it instead! 😊")]
    public async Task<RuntimeResult> GetRoleAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 `role` 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
