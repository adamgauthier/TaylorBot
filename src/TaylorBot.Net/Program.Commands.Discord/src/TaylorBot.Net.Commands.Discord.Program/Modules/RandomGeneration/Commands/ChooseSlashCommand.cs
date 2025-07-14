using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Random;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;

public class ChooseSlashCommand(ICryptoSecureRandom cryptoSecureRandom, CommandMentioner mention) : ISlashCommand<ChooseSlashCommand.Options>
{
    public static string CommandName => "choose";

    public static readonly CommandMetadata Metadata = new(CommandName);

    public Command Choose(string options, RunContext context) => new(
        context.SlashCommand != null ? Metadata : Metadata with { IsSlashCommand = false },
        () =>
        {
            var parsedOptions = options.Split(',').Select(o => o.Trim()).Where(o => !string.IsNullOrWhiteSpace(o)).ToList();

            var randomOption = cryptoSecureRandom.GetRandomElement(parsedOptions);

            var description = new List<string> { randomOption };
            EmbedBuilder embed = new();

            if (context.SlashCommand == null)
            {
                description.AddRange(["", $"Use {mention.SlashCommand("choose", context)} instead! 😊"]);
            }

            embed
                .WithColor(TaylorBotColors.SuccessColor)
                .WithTitle("I choose:")
                .WithDescription(string.Join('\n', description));

            return new(new EmbedResult(embed.Build()));
        }
    );

    public ISlashCommandInfo Info => new MessageCommandInfo(Metadata.Name);

    public record Options(ParsedString options);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Choose(options.options.Value, context));
    }
}
