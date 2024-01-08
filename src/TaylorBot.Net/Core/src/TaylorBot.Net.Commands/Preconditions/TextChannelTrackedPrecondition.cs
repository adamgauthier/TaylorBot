using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Preconditions;

public class TextChannelTrackedPrecondition(ISpamChannelRepository spamChannelRepository) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild != null)
        {
            var textChannel = context.Channel.CreateLegacyTextChannel(context.Guild);
            await spamChannelRepository.InsertOrGetIsSpamChannelAsync(textChannel);
        }

        return new PreconditionPassed();
    }
}
