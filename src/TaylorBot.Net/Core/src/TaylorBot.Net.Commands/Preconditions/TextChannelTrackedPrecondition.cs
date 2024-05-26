using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Preconditions;

public class TextChannelTrackedPrecondition(ISpamChannelRepository spamChannelRepository) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild?.Fetched != null && context.GuildTextChannel != null)
        {
            _ = await spamChannelRepository.InsertOrGetIsSpamChannelAsync(context.GuildTextChannel);
        }

        return new PreconditionPassed();
    }
}
