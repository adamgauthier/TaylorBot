using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Commands;

[Name("Reminders ⏰")]
public class RemindModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("remindme")]
    [Alias("reminder")]
    public async Task<RuntimeResult> RemindAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: RemindAddSlashCommand.CommandName, IsRemoved: true));

    [Command("clearreminders")]
    [Alias("clearreminder", "cr")]
    public async Task<RuntimeResult> ClearRemindersAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: RemindManageSlashCommand.CommandName, IsRemoved: true));
}
