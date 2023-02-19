using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.Preconditions
{
    public interface IDisabledGuildCommandRepository
    {
        ValueTask<bool> IsGuildCommandDisabledAsync(IGuild guild, CommandMetadata command);
        ValueTask EnableInAsync(IGuild guild, string commandName);
        ValueTask DisableInAsync(IGuild guild, string commandName);
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

            var canRun = await new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild).CanRunAsync(command, context);

            return isDisabled ?
                new PreconditionFailed(
                    PrivateReason: $"{command.Metadata.Name} is disabled in {context.Guild.FormatLog()}",
                    UserReason: new(
                        string.Join('\n',
                            new[] {
                                $"You can't use `{command.Metadata.Name}` because it is disabled in this server.",
                                canRun is PreconditionPassed
                                    ? $"You can re-enable it by typing </command server-enable:909694280703016991> {command.Metadata.Name}."
                                    : "Ask a moderator to re-enable it."
                            }),
                        HideInPrefixCommands: true
                    )
                ) :
                new PreconditionPassed();
        }
    }
}
