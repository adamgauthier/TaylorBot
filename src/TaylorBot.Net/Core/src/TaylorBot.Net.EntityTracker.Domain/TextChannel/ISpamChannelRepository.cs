using Discord;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.EntityTracker.Domain.TextChannel;

public record GuildTextChannel(SnowflakeId Id, SnowflakeId GuildId, ChannelType Type)
{
    public string Mention => MentionUtils.MentionChannel(Id);
}

public interface ISpamChannelRepository
{
    ValueTask<bool> InsertOrGetIsSpamChannelAsync(GuildTextChannel channel);
    ValueTask AddSpamChannelAsync(GuildTextChannel channel);
    ValueTask RemoveSpamChannelAsync(GuildTextChannel channel);
}
