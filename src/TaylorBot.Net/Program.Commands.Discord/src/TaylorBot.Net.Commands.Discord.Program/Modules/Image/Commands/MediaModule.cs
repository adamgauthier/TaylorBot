using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;

[Name("Media 📷")]
public class MediaModule(ICommandRunner commandRunner, ImageSlashCommand imageCommand) : TaylorBotModule
{
    [Command("image")]
    [Alias("imagen")]
    [Summary("Searches images based on the search text provided.")]
    public async Task<RuntimeResult> ImageAsync(
        [Remainder]
        string text
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            imageCommand.Image(context.User, text, isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }
}
