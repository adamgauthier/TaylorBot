using Discord.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Commands;

[Name("Log 🪵")]
[Group("log")]
public class LogModule : ModuleBase
{
    [Name("Deleted Logs 🗑")]
    [Group("deleted")]
    public class DeletedModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
    {
        [Priority(-1)]
        [Command]
        public async Task<RuntimeResult> AddAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
            Context,
            new(ReplacementSlashCommand: MonitorDeletedSetSlashCommand.CommandName, IsRemoved: true));

        [Command("stop")]
        public async Task<RuntimeResult> RemoveAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
            Context,
            new(ReplacementSlashCommand: MonitorDeletedShowSlashCommand.CommandName, IsRemoved: true));
    }

    [Name("Member Logs 🧍")]
    [Group("member")]
    public class MemberModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
    {
        [Priority(-1)]
        [Command]
        public async Task<RuntimeResult> AddAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
            Context,
            new(ReplacementSlashCommand: MonitorMembersSetSlashCommand.CommandName, IsRemoved: true));

        [Command("stop")]
        public async Task<RuntimeResult> RemoveAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
            Context,
            new(ReplacementSlashCommand: MonitorMembersShowSlashCommand.CommandName, IsRemoved: true));
    }
}
