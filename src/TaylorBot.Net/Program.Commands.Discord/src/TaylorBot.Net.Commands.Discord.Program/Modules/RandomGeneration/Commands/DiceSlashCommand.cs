using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Random;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

public class DiceSlashCommand(ICryptoSecureRandom cryptoSecureRandom) : ISlashCommand<DiceSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("dice");

    public record Options(ParsedPositiveInteger faces);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            () =>
            {
                var faces = options.faces.Value;
                ArgumentOutOfRangeException.ThrowIfNegative(faces);

                var randomNumber = cryptoSecureRandom.GetInt32(1, faces);

                return new(new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle($"Rolling a dice with {"face".ToQuantity(faces, TaylorBotFormats.Readable)} 🎲")
                    .WithDescription($"You rolled {randomNumber.ToString(TaylorBotFormats.BoldReadable)}!")
                .Build()));
            }
        ));
    }
}
