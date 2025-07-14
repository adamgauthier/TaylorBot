using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Commands;

[Name("Usernames 🏷️")]
[Group("usernames")]
[Alias("names")]
public class UsernamesModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Priority(-1)]
    [Command]
    public async Task<RuntimeResult> GetAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: UsernamesShowSlashCommand.CommandName, IsRemoved: true));

    [Command("private")]
    public async Task<RuntimeResult> PrivateAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: UsernamesVisibilitySlashCommand.CommandName, IsRemoved: true));

    [Command("public")]
    public async Task<RuntimeResult> PublicAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: UsernamesVisibilitySlashCommand.CommandName, IsRemoved: true));
}
