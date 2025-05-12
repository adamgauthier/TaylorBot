using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;

[Name("Media 📷")]
public class MediaModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Command("image")]
    [Alias("imagen")]
    [Summary("This command has been moved to 👉 </image:870731803739168860> 👈 Please use it instead! 😊")]
    public async Task<RuntimeResult> ImageAsync(
        [Remainder]
        string? _ = null)
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </image:870731803739168860> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
