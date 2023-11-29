using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Preconditions
{
    public class TextChannelTrackedPrecondition : ICommandPrecondition
    {
        private readonly ISpamChannelRepository _spamChannelRepository;

        public TextChannelTrackedPrecondition(ISpamChannelRepository spamChannelRepository)
        {
            _spamChannelRepository = spamChannelRepository;
        }

        public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
        {
            if (context.Guild != null)
            {
                var textChannel = context.Channel.CreateLegacyTextChannel(context.Guild);
                await _spamChannelRepository.InsertOrGetIsSpamChannelAsync(textChannel);
            }

            return new PreconditionPassed();
        }
    }
}
