using Discord.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Commands.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Commands;

[Name("Admin 📜")]
public class AdminModule(ICommandRunner commandRunner, PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("addlogchannel")]
    [Alias("alc")]
    public async Task<RuntimeResult> AddLogAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: MonitorMembersSetSlashCommand.CommandName, IsRemoved: true));

    [Command("removelogchannel")]
    [Alias("rlc")]
    public async Task<RuntimeResult> RemoveLogAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: MonitorMembersShowSlashCommand.CommandName, IsRemoved: true));

    [Command("addspamchannel")]
    [Alias("asc")]
    public async Task<RuntimeResult> AddSpamAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: ModSpamAddSlashCommand.CommandName, IsRemoved: true));

    [Command("removespamchannel")]
    [Alias("rsc")]
    public async Task<RuntimeResult> RemoveSpamAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: ModSpamRemoveSlashCommand.CommandName, IsRemoved: true));

    [Command("disablechannelcommand")]
    [Alias("dcc")]
    public async Task<RuntimeResult> DisableChannelCommandAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: CommandChannelDisableSlashCommand.CommandName, IsRemoved: true));

    [Command("enablechannelcommand")]
    [Alias("ecc")]
    public async Task<RuntimeResult> EnableChannelCommandAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: CommandChannelEnableSlashCommand.CommandName, IsRemoved: true));

    [Command("disableservercommand")]
    [Alias("disableguildcommand", "dgc", "dsc")]
    public async Task<RuntimeResult> DisableServerCommandAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: CommandServerDisableSlashCommand.CommandName, IsRemoved: true));

    [Command("enableservercommand")]
    [Alias("enableguildcommand", "egc", "esc")]
    public async Task<RuntimeResult> EnableServerCommandAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: CommandServerEnableSlashCommand.CommandName, IsRemoved: true));

    [Command("addaccessiblerole")]
    [Alias("aar")]
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

        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("removeaccessiblerole")]
    [Alias("rar")]
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

        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("droprole")]
    [Alias("dr")]
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

        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("getrole")]
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

        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
