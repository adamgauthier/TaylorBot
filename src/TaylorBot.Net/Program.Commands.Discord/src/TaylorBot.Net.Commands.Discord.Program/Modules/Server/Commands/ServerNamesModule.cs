using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

[Name("Stats 📊")]
public class ServerNamesModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("servernames")]
    [Alias("snames", "guildnames", "gnames")]
    public async Task<RuntimeResult> ServerNamesAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: ServerNamesSlashCommand.CommandName, IsRemoved: true));

    [Command("profile")]
    [Alias("info", "asl")]
    public async Task<RuntimeResult> ProfileAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommands: ["birthday age", "location show", "gender show"], IsRemoved: true));
}
