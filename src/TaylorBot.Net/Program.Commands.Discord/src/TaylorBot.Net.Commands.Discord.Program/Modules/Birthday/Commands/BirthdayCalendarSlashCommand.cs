using Discord;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands
{
    public class BirthdayCalendarSlashCommand : ISlashCommand<NoOptions>
    {
        public SlashCommandInfo Info => new("birthday calendar");

        private readonly IBirthdayRepository _birthdayRepository;

        public BirthdayCalendarSlashCommand(IBirthdayRepository birthdayRepository)
        {
            _birthdayRepository = birthdayRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    var guild = context.Guild!;
                    var calendar = await _birthdayRepository.GetBirthdayCalendarAsync(guild);

                    var pages = calendar.Chunk(15).Select(entries => string.Join('\n', entries.Select(
                        entry => $"{entry.Username.MdUserLink(entry.UserId)} - {entry.NextBirthday.ToString("MMMM d", TaylorBotCulture.Culture)}"
                    ))).ToList();

                    var baseEmbed = new EmbedBuilder()
                        .WithGuildAsAuthor(guild)
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithTitle("Upcoming Birthdays");

                    return new PageMessageResultBuilder(new(
                        new(new EmbedDescriptionTextEditor(
                            baseEmbed,
                            pages,
                            hasPageFooter: true,
                            emptyText: string.Join('\n', new[] {
                                "No upcoming birthdays in this server.",
                                $"Members need to use {context.MentionCommand("birthday set")}! 😊"
                            })
                        )),
                        IsCancellable: true
                    )).Build();
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                }
            ));
        }
    }
}
