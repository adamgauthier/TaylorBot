using Discord;

namespace TaylorBot.Net.EntityTracker.Domain.TextChannel;

public interface ISpamChannelRepository
{
    ValueTask<bool> InsertOrGetIsSpamChannelAsync(ITextChannel channel);
    ValueTask AddSpamChannelAsync(ITextChannel channel);
    ValueTask RemoveSpamChannelAsync(ITextChannel channel);
}
