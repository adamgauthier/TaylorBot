using Discord;
using System.Threading.Tasks;
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
            if (context.Channel is ITextChannel textChannel)
            {
                await _spamChannelRepository.InsertOrGetIsSpamChannelAsync(textChannel);
            }

            return new PreconditionPassed();
        }
    }
}
