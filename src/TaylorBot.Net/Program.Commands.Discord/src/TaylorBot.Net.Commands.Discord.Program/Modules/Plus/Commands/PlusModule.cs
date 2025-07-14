using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Commands;

[Name("TaylorBot Plus 💎")]
[Group("plus")]
[Alias("support", "patreon", "donate")]
public class PlusModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Priority(-1)]
    [Command]
    public async Task<RuntimeResult> PlusAsync() => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: PlusShowSlashCommand.CommandName, IsRemoved: true));

    [Command("add")]
    public async Task<RuntimeResult> AddAsync() => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: PlusAddSlashCommand.CommandName, IsRemoved: true));

    [Command("remove")]
    public async Task<RuntimeResult> RemoveAsync() => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: PlusRemoveSlashCommand.CommandName, IsRemoved: true));
}
