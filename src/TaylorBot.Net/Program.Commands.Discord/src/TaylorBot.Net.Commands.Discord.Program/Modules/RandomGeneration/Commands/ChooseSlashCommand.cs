using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Random;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;

public class ChooseSlashCommand(ICryptoSecureRandom cryptoSecureRandom, CommandMentioner mention) : ISlashCommand<ChooseSlashCommand.Options>
{
    public static string CommandName => "choose";

    public static readonly CommandMetadata Metadata = new(CommandName);

    public Command Choose(string options, RunContext context) => new(
        context.SlashCommand != null ? Metadata : Metadata with { IsSlashCommand = false },
        async () =>
        {
            var parsedOptions = options.Split(',').Select(o => o.Trim()).Where(o => !string.IsNullOrWhiteSpace(o)).ToList();

            if (parsedOptions.Count == 0)
            {
                return new EmbedResult(EmbedFactory.CreateError(
                    "Please provide at least one option to choose from! 😊"
                ));
            }

            var randomOption = cryptoSecureRandom.GetRandomElement(parsedOptions);

            List<string> description = [randomOption];
            EmbedBuilder embed = new();

            if (context.SlashCommand == null)
            {
                description.AddRange(["", await mention.FormatAsync(context, $"Use {mention.Slash("choose")} instead! 😊")]);
            }

            embed
                .WithColor(TaylorBotColors.SuccessColor)
                .WithTitle("I choose:")
                .WithDescription(string.Join('\n', description));

            return new EmbedResult(embed.Build());
        }
    );

    public ISlashCommandInfo Info => new MessageCommandInfo(Metadata.Name);

    public record Options(ParsedString options);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Choose(options.options.Value, context));
    }
}
