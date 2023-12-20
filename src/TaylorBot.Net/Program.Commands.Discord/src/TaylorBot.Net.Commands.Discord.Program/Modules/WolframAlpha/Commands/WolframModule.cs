using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Commands;

[Name("Wolfram 🤖")]
public class WolframModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Command("wolfram")]
    [Alias("wolframalpha", "wa")]
    [Summary("This command has been moved to </wolframalpha:1082193237210574910>. Please use it instead! 😊")]
    public async Task<RuntimeResult> WolframAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </wolframalpha:1082193237210574910> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
