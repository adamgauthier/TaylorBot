using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Preconditions;

public class TextChannelTrackedPrecondition(ISpamChannelRepository spamChannelRepository) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild != null)
        {
            await spamChannelRepository.InsertOrGetIsSpamChannelAsync(new(context.Channel.Id, context.Guild.Id));
        }

        return new PreconditionPassed();
    }
}
