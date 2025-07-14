using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Commands;

[Name("TaypointWill")]
[Group("taypointwill")]
public class TaypointWillModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Priority(-1)]
    [Command]
    public async Task<RuntimeResult> GetAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: TaypointsSuccessionSlashCommand.CommandName, IsRemoved: true));

    [Command("add")]
    public async Task<RuntimeResult> AddAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: TaypointsSuccessionSlashCommand.CommandName, IsRemoved: true));

    [Command("clear")]
    public async Task<RuntimeResult> ClearAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: TaypointsSuccessionSlashCommand.CommandName, IsRemoved: true));

    [Command("claim")]
    public async Task<RuntimeResult> ClaimAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: TaypointsSuccessionSlashCommand.CommandName, IsRemoved: true));
}
