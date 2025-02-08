using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Commands;

[Name("YouTube")]
public class YouTubeModule(ICommandRunner commandRunner, YouTubeSlashCommand youTubeCommand) : TaylorBotModule
{
    [Command("youtube")]
    [Alias("yt")]
    [Summary("Search videos on YouTube")]
    public async Task<RuntimeResult> SearchAsync(
        [Summary("What search text would you like to get YouTube results for?")]
        [Remainder]
        string text
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            youTubeCommand.Search(context.User, text, isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }
}
