using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Commands;

public class UsernamesShowSlashCommand(IUsernameHistoryRepository usernameHistoryRepository, CommandMentioner mention, PageMessageFactory pageMessageFactory) : ISlashCommand<UsernamesShowSlashCommand.Options>
{
    public static string CommandName => "usernames show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public Command Show(DiscordUser user, RunContext context) => new(
        new(Info.Name),
        async () =>
        {
            EmbedBuilder BuildBaseEmbed() =>
                new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor).WithUserAsAuthor(user);

            if (await usernameHistoryRepository.IsUsernameHistoryHiddenFor(user))
            {
                return new EmbedResult(BuildBaseEmbed()
                    .WithDescription(
                        await mention.FormatAsync(context, $"""
                        {user.Mention}'s username history is **private** and can't be viewed 🕵️
                        Use {mention.Slash("usernames visibility")} to change your username history visibility 🫣
                        """))
                .Build());
            }
            else
            {
                var usernames = await usernameHistoryRepository.GetUsernameHistoryFor(user, count: 75);

                var usernamesAsLines = usernames.Select(u => $"{u.ChangedAt.FormatLongDate()}: {u.Username}");

                var pages = usernamesAsLines.Chunk(15)
                    .Select(lines => string.Join('\n', lines))
                    .ToList();

                return pageMessageFactory.Create(new(
                    new(new EmbedDescriptionTextEditor(
                        BuildBaseEmbed(),
                        pages,
                        hasPageFooter: true,
                        emptyText:
                            """
                            No username history for this user 🤔
                            """
                    )),
                    IsCancellable: true));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Show(options.user.User, context));
    }
}
