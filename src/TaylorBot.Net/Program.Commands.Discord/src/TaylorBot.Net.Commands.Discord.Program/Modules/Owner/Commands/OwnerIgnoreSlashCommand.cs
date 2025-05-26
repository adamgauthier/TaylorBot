using Humanizer;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Owner.Commands;

public class OwnerIgnoreSlashCommand(
    IIgnoredUserRepository ignoredUserRepository,
    TaylorBotOwnerPrecondition ownerPrecondition,
    TimeProvider timeProvider) : ISlashCommand<OwnerIgnoreSlashCommand.Options>
{
    public static string CommandName => "owner ignore";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserNotAuthorAndTaylorBot user, ParsedTimeSpan time);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await ignoredUserRepository.IgnoreUntilAsync(options.user.User, timeProvider.GetUtcNow() + options.time.Value);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"Ignoring {options.user.User.FormatTagAndMention()} for **{options.time.Value.Humanize(culture: TaylorBotCulture.Culture)}** 👍"
                ));
            },
            Preconditions: [
                ownerPrecondition
            ]
        ));
    }
}
