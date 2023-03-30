using Humanizer;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Owner.Commands
{
    public class OwnerIgnoreSlashCommand : ISlashCommand<OwnerIgnoreSlashCommand.Options>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("owner ignore");

        public record Options(ParsedUserNotAuthorAndTaylorBot user, ParsedTimeSpan time);

        private readonly IIgnoredUserRepository _ignoredUserRepository;

        public OwnerIgnoreSlashCommand(IIgnoredUserRepository ignoredUserRepository)
        {
            _ignoredUserRepository = ignoredUserRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    await _ignoredUserRepository.IgnoreUntilAsync(options.user.User, DateTimeOffset.Now + options.time.Value);

                    return new EmbedResult(EmbedFactory.CreateSuccess(
                        $"Ignoring {options.user.User.FormatTagAndMention()} for **{options.time.Value.Humanize(culture: TaylorBotCulture.Culture)}**. 👍"
                    ));
                },
                Preconditions: new ICommandPrecondition[] {
                    new TaylorBotOwnerPrecondition()
                }
            ));
        }
    }
}
