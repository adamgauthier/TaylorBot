using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Channel.Commands;

[Name("Stats 📊")]
public class ChannelStatsModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("channelstats")]
    [Alias("cstats")]
    public async Task<RuntimeResult> ChannelStatsAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: ChannelMessagesSlashCommand.CommandName, IsRemoved: true));
}
