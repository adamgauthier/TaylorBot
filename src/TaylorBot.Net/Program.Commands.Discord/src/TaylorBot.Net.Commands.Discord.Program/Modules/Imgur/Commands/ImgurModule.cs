using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Commands;

[Name("Media 📷")]
public class ImgurModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Command("imgur")]
    [Summary("This command has been moved to </imgur:1101376729135726652>. Please use it instead! 😊")]
    public async Task<RuntimeResult> ImgurAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </imgur:1101376729135726652> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
