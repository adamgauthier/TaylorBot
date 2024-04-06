using Discord;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.EntityTracker.Domain.TextChannel;

public record GuildTextChannel(SnowflakeId Id, SnowflakeId GuildId);

public interface ISpamChannelRepository
{
    ValueTask<bool> InsertOrGetIsSpamChannelAsync(GuildTextChannel channel);
    ValueTask AddSpamChannelAsync(ITextChannel channel);
    ValueTask RemoveSpamChannelAsync(ITextChannel channel);
}
