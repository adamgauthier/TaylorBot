using Discord.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Commands;

[Name("Stats 📊")]
public class StatsModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("serverstats")]
    [Alias("sstats", "genderstats", "agestats")]
    public async Task<RuntimeResult> ServerStatsAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: ServerPopulationSlashCommand.CommandName, IsRemoved: true));
}
