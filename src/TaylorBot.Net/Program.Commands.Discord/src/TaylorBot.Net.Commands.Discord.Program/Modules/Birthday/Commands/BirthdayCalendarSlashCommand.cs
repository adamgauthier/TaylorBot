using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;

public class BirthdayCalendarSlashCommand(IBirthdayRepository birthdayRepository, MemberNotInGuildUpdater memberNotInGuildUpdater) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("birthday calendar");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                ArgumentNullException.ThrowIfNull(context.Guild);
                var guild = context.Guild;

                var calendar = await birthdayRepository.GetBirthdayCalendarAsync(guild);

                if (guild.Fetched != null)
                {
                    memberNotInGuildUpdater.UpdateMembersWhoLeftInBackground(
                        nameof(BirthdayCalendarSlashCommand),
                        guild.Fetched,
                        calendar.Select(e => e.UserId).ToList());
                }

                var pages = calendar.Chunk(15).Select(entries => string.Join('\n', entries.Select(
                    entry => $"{entry.Username.MdUserLink(entry.UserId)} - {entry.NextBirthday.ToString("MMMM d", TaylorBotCulture.Culture)}"
                ))).ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Upcoming Birthdays");

                if (guild.Fetched != null)
                {
                    baseEmbed.WithGuildAsAuthor(guild.Fetched);
                }

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText:
                            $"""
                            No upcoming birthdays in this server for the next 6 months.
                            Members need to use {context.MentionCommand("birthday set")}! 😊
                            """
                    )),
                    IsCancellable: true
                )).Build();
            },
            Preconditions: [new InGuildPrecondition()]
        ));
    }
}
