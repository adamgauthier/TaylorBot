using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Random;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;

public class ChooseSlashCommand(ICryptoSecureRandom cryptoSecureRandom) : ISlashCommand<ChooseSlashCommand.Options>
{
    public static string CommandName => "choose";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString options);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            () =>
            {
                var parsedOptions = options.options.Value.Split(',').Select(o => o.Trim()).Where(o => !string.IsNullOrWhiteSpace(o)).ToList();

                if (parsedOptions.Count == 0)
                {
                    return new(new EmbedResult(EmbedFactory.CreateError(
                        "Please provide at least one option to choose from! 😊"
                    )));
                }

                var randomOption = cryptoSecureRandom.GetRandomElement(parsedOptions);

                return new(new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("🎲 I choose:")
                    .WithDescription($"## {FormatOption(randomOption)}".Truncate(EmbedBuilder.MaxDescriptionLength))
                    .AddField($"🎩 Options ({parsedOptions.Count})",
                        string.Join(", ", parsedOptions.Select(FormatOption)).Truncate(EmbedFieldBuilder.MaxFieldValueLength))
                    .Build()));
            }
        ));
    }

    private static string FormatOption(string option) =>
        option.ReplaceLineEndings(" ").EscapeDiscordMarkdown();
}
