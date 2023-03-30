using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands
{
    public class ChooseCommand
    {
        public static readonly CommandMetadata Metadata = new("choose", "Random 🎲");

        private readonly ICryptoSecureRandom _cryptoSecureRandom;

        public ChooseCommand(ICryptoSecureRandom cryptoSecureRandom)
        {
            _cryptoSecureRandom = cryptoSecureRandom;
        }

        public Command Choose(string options, IUser? author = null) => new(
            Metadata,
            () =>
            {
                var parsedOptions = options.Split(',').Select(o => o.Trim()).Where(o => !string.IsNullOrWhiteSpace(o)).ToList();

                var randomOption = _cryptoSecureRandom.GetRandomElement(parsedOptions);

                var description = new List<string> { randomOption };
                var embed = new EmbedBuilder();

                if (author != null)
                {
                    embed.WithUserAsAuthor(author);
                    description.AddRange(new[] { "", "Use </choose:843563366751404063> instead! 😊" });
                }

                embed
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("I choose:")
                    .WithDescription(string.Join('\n', description));

                return new(new EmbedResult(embed.Build()));
            }
        );
    }

    public class ChooseSlashCommand : ISlashCommand<ChooseSlashCommand.Options>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo(ChooseCommand.Metadata.Name);

        public record Options(ParsedString options);

        private readonly ICryptoSecureRandom _cryptoSecureRandom;

        public ChooseSlashCommand(ICryptoSecureRandom cryptoSecureRandom)
        {
            _cryptoSecureRandom = cryptoSecureRandom;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new ChooseCommand(_cryptoSecureRandom).Choose(options.options.Value));
        }
    }
}
