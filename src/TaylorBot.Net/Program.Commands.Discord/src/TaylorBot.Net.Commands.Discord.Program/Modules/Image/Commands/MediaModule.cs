using Discord.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;

[Name("Media 📷")]
public class MediaModule(ICommandRunner commandRunner, IPlusRepository plusRepository, IRateLimiter rateLimiter, IImageSearchClient imageSearchClient) : TaylorBotModule
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
        var command = new ImageCommand(plusRepository, rateLimiter, imageSearchClient);
        var result = await commandRunner.RunAsync(
            command.Image(Context.User, text, isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }
}
