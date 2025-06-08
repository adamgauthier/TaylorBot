using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Commands;

[Name("Stats 📊")]
public class StatsModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Command("serverstats")]
    [Alias("sstats", "genderstats", "agestats")]
    [Summary("Gets age and gender stats for a server.")]
    public async Task<RuntimeResult> ServerStatsAsync(
        [Remainder]
        string? _ = null)
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </server population:1137547317549998130> 👈
                Please use it instead! 😊
                """
            )))
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
