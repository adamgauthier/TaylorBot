using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Commands;

public class UsernamesShowSlashCommand(IUsernameHistoryRepository usernameHistoryRepository) : ISlashCommand<UsernamesShowSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("usernames show");

    public record Options(ParsedUserOrAuthor user);

    public Command Show(DiscordUser user, RunContext? context = null) => new(
        new(Info.Name),
        async () =>
        {
            EmbedBuilder BuildBaseEmbed() =>
                new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor).WithUserAsAuthor(user);

            if (await usernameHistoryRepository.IsUsernameHistoryHiddenFor(user))
            {
                return new EmbedResult(BuildBaseEmbed()
                    .WithDescription(
                        $"""
                        {user.Mention}'s username history is **private** and can't be viewed 🕵️
                        Use {context?.MentionCommand("usernames visibility") ?? "</usernames visibility:1214813880463921242>"} to change your username history visibility 🫣
                        """)
                .Build());
            }
            else
            {
                var usernames = await usernameHistoryRepository.GetUsernameHistoryFor(user, 75);

                var usernamesAsLines = usernames.Select(u => $"{u.ChangedAt.FormatLongDate()}: {u.Username}");

                var pages = usernamesAsLines.Chunk(size: 15)
                    .Select(lines => string.Join('\n', lines))
                    .ToList();

                if (context != null)
                {
                    return new PageMessageResultBuilder(new(
                        new(new EmbedDescriptionTextEditor(
                            BuildBaseEmbed(),
                            pages,
                            hasPageFooter: true,
                            emptyText:
                                """
                                No username history for this user 🤔
                                """
                        )),
                        IsCancellable: true
                    )).Build();
                }
                else
                {
                    return new PageMessageResult(new PageMessage(new(
                        new EmbedPageMessageRenderer(new DescriptionPageEditor(pages), BuildBaseEmbed)
                    )));
                }
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Show(options.user.User, context));
    }
}
