using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;

[Name("Random 🎲")]
public class RandomModule(ICommandRunner commandRunner, ChooseSlashCommand chooseSlashCommand) : TaylorBotModule
{
    [Command("dice")]
    [Summary("This command has been moved to **/dice**. Please use it instead! 😊")]
    public async Task<RuntimeResult> DiceAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 **/dice** 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("choose")]
    [Alias("choice")]
    [Summary("Chooses a random option from a list.")]
    public async Task<RuntimeResult> ChooseAsync(
        [Remainder]
        [Summary("What are the options (comma separated) to choose from?")]
        string options
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(
            chooseSlashCommand.Choose(options, Context.User),
            context
        );

        return new TaylorBotResult(result, context);
    }
}
