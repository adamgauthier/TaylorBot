using Discord.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;

[Name("Media 📷")]
public class MediaModule : TaylorBotModule
{
    private readonly ICommandRunner _commandRunner;
    private readonly IPlusRepository _plusRepository;
    private readonly IRateLimiter _rateLimiter;
    private readonly IImageSearchClient _imageSearchClient;

    public MediaModule(ICommandRunner commandRunner, IPlusRepository plusRepository, IRateLimiter rateLimiter, IImageSearchClient imageSearchClient)
    {
        _commandRunner = commandRunner;
        _plusRepository = plusRepository;
        _rateLimiter = rateLimiter;
        _imageSearchClient = imageSearchClient;
    }

    [Command("image")]
    [Alias("imagen")]
    [Summary("Searches images based on the search text provided.")]
    public async Task<RuntimeResult> ImageAsync(
        [Remainder]
        string text
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var command = new ImageCommand(_plusRepository, _rateLimiter, _imageSearchClient);
        var result = await _commandRunner.RunAsync(
            command.Image(Context.User, text, isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }
}
