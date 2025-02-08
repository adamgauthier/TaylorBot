using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Commands;

[Name("Stats 📊")]
public class ChannelStatsModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Command("channelstats")]
    [Alias("cstats")]
    [Summary("This command has been moved to </channel messages:1139070407765409912>. Please use it instead! 😊")]
    public async Task<RuntimeResult> ChannelStatsAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </channel messages:1139070407765409912> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
