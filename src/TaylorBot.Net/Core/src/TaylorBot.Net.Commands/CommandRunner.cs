using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands
{
    public record RunContext(DateTimeOffset CreatedAt, IUser User, IMessageChannel Channel, IGuild? Guild, IDiscordClient Client, string CommandPrefix, OnGoingState OnGoingState);
    public class OnGoingState { public string? OnGoingCommandAddedToPool { get; set; } }

    public record Command(CommandMetadata Metadata, Func<ValueTask<ICommandResult>> RunAsync);
    public record CommandMetadata(string Name, string? ModuleName = null, IReadOnlyList<string>? Aliases = null);

    public interface ICommandRunner
    {
        ValueTask<ICommandResult> RunAsync(Command command, RunContext context, IList<ICommandPrecondition>? additionalPreconditions = null);
    }

    public class CommandRunner : ICommandRunner
    {
        private readonly List<ICommandPrecondition> _preconditions;

        public CommandRunner(NotDisabledPrecondition notDisabled, NotGuildDisabledPrecondition notGuildDisabled, NotGuildChannelDisabledPrecondition notGuildChannelDisabled, UserNotIgnoredPrecondition userNotIgnored, MemberTrackedPrecondition memberTracked, TextChannelTrackedPrecondition textChannelTracked, UserNoOngoingCommandPrecondition userNoOngoingCommand)
        {
            _preconditions = new() { notDisabled, notGuildDisabled, notGuildChannelDisabled, userNotIgnored, memberTracked, textChannelTracked, userNoOngoingCommand };
        }

        public async ValueTask<ICommandResult> RunAsync(Command command, RunContext context, IList<ICommandPrecondition>? additionalPreconditions = null)
        {
            foreach (var precondition in _preconditions.Concat(additionalPreconditions ?? Array.Empty<ICommandPrecondition>()))
            {
                if (await precondition.CanRunAsync(command, context) is PreconditionFailed failed)
                    return failed;
            }

            var result = await command.RunAsync();

            return result;
        }
    }
}
