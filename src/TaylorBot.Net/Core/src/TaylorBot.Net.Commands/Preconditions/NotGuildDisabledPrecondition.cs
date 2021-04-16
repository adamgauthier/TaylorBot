using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.Preconditions
{
    public interface IDisabledGuildCommandRepository
    {
        ValueTask<bool> IsGuildCommandDisabledAsync(IGuild guild, CommandMetadata command);
    }

    public class NotGuildDisabledPrecondition : ICommandPrecondition
    {
        private readonly IDisabledGuildCommandRepository _disabledGuildCommandRepository;

        public NotGuildDisabledPrecondition(IDisabledGuildCommandRepository disabledGuildCommandRepository)
        {
            _disabledGuildCommandRepository = disabledGuildCommandRepository;
        }

        public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
        {
            if (context.Guild == null)
                return new PreconditionPassed();

            var isDisabled = await _disabledGuildCommandRepository.IsGuildCommandDisabledAsync(context.Guild, command.Metadata);

            return isDisabled ?
                new PreconditionFailed(
                    PrivateReason: $"{command.Metadata.Name} is disabled in {context.Guild.FormatLog()}",
                    UserReason: new(
                        string.Join('\n',
                            new[] {
                                $"You can't use `{command.Metadata.Name}` because it is disabled in this server.",
                                $"You can re-enable it by typing `{context.CommandPrefix}esc {command.Metadata.Name}`."
                            }),
                        HideInPrefixCommands: true
                    )
                ) :
                new PreconditionPassed();
        }
    }
}
