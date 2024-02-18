using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Random;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;

public class ChooseCommand(ICryptoSecureRandom cryptoSecureRandom)
{
    public static readonly CommandMetadata Metadata = new("choose", "Random 🎲");

    public Command Choose(string options, IUser? author = null) => new(
        Metadata,
        () =>
        {
            var parsedOptions = options.Split(',').Select(o => o.Trim()).Where(o => !string.IsNullOrWhiteSpace(o)).ToList();

            var randomOption = cryptoSecureRandom.GetRandomElement(parsedOptions);

            var description = new List<string> { randomOption };
            var embed = new EmbedBuilder();

            if (author != null)
            {
                description.AddRange(["", "Use </choose:843563366751404063> instead! 😊"]);
            }

            embed
                .WithColor(TaylorBotColors.SuccessColor)
                .WithTitle("I choose:")
                .WithDescription(string.Join('\n', description));

            return new(new EmbedResult(embed.Build()));
        }
    );
}

public class ChooseSlashCommand(ICryptoSecureRandom cryptoSecureRandom) : ISlashCommand<ChooseSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo(ChooseCommand.Metadata.Name);

    public record Options(ParsedString options);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new ChooseCommand(cryptoSecureRandom).Choose(options.options.Value));
    }
}
