using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Commands;

[Name("Reminders ⏰")]
public class RemindModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Command("remindme")]
    [Alias("reminder")]
    [Summary("This command has been moved to </remind add:861754955728027678>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RemindAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </remind add:861754955728027678> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("clearreminders")]
    [Alias("clearreminder", "cr")]
    [Summary("This command has been moved to </remind manage:861754955728027678>. Please use it instead! 😊")]
    public async Task<RuntimeResult> ClearRemindersAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </remind manage:861754955728027678> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
