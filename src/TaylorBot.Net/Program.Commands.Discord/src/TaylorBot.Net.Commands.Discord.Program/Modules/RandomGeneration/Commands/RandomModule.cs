using Discord;
using Discord.Commands;
using Humanizer;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Random;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;

[Name("Random 🎲")]
public class RandomModule(ICommandRunner commandRunner, ICryptoSecureRandom cryptoSecureRandom) : TaylorBotModule
{
    [Command("dice")]
    [Summary("Rolls a dice with the specified amount of faces.")]
    public async Task<RuntimeResult> DiceAsync(
        [Remainder]
        [Summary("How many faces should your dice have?")]
        PositiveInt32 faces
    )
    {
        var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), () =>
        {
            var randomNumber = cryptoSecureRandom.GetInt32(1, faces.Parsed);

            return new(new EmbedResult(new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithTitle($"Rolling a dice with {"face".ToQuantity(faces.Parsed, TaylorBotFormats.Readable)} 🎲")
                .WithDescription($"You rolled {randomNumber.ToString(TaylorBotFormats.BoldReadable)}!")
            .Build()));
        });

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
            new ChooseCommand(cryptoSecureRandom).Choose(options, Context.User),
            context
        );

        return new TaylorBotResult(result, context);
    }
}
