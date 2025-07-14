using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Commands;

[Name("YouTube")]
public class YouTubeModule(ICommandRunner commandRunner, YouTubeSlashCommand youTubeCommand) : TaylorBotModule
{
    [Command("youtube")]
    [Alias("yt")]
    public async Task<RuntimeResult> SearchAsync(
        [Remainder]
        string text
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: YouTubeSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            youTubeCommand.Search(context.User, text, context),
            context
        );

        return new TaylorBotResult(result, context);
    }
}
