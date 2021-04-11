using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.Preconditions
{
    public interface IDisabledGuildChannelCommandRepository
    {
        ValueTask<bool> IsGuildChannelCommandDisabledAsync(ITextChannel textChannel, CommandMetadata command);
    }

    public class NotGuildChannelDisabledPrecondition : ICommandPrecondition
    {
        private readonly IDisabledGuildChannelCommandRepository _disabledGuildChannelCommandRepository;

        public NotGuildChannelDisabledPrecondition(IDisabledGuildChannelCommandRepository disabledGuildChannelCommandRepository)
        {
            _disabledGuildChannelCommandRepository = disabledGuildChannelCommandRepository;
        }

        public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
        {
            if (context.Channel is not ITextChannel textChannel)
                return new PreconditionPassed();

            var isDisabled = await _disabledGuildChannelCommandRepository.IsGuildChannelCommandDisabledAsync(textChannel, command.Metadata);

            return isDisabled ?
                new PreconditionFailed(
                    PrivateReason: $"{command.Metadata.Name} is disabled in {textChannel.FormatLog()}",
                    UserReason: string.Join('\n', new[] {
                        $"You can't use `{command.Metadata.Name}` because it is disabled in {textChannel.Mention}.",
                        $"You can re-enable it by typing `{context.CommandPrefix}ecc {command.Metadata.Name}`."
                    })
                ) :
                new PreconditionPassed();
        }
    }
}
