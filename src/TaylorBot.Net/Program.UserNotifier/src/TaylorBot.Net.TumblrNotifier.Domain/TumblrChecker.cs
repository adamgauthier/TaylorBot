using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.TumblrNotifier.Domain;

public class TumblrChecker(SnowflakeId guildId, SnowflakeId channelId, string blogName, string? lastPostShortUrl)
{
    public SnowflakeId GuildId { get; } = guildId;
    public SnowflakeId ChannelId { get; } = channelId;
    public string BlogName { get; } = blogName;
    public string? LastPostShortUrl { get; } = lastPostShortUrl;

    public override string ToString()
    {
        return $"Tumblr Checker for Guild {GuildId}, Channel {ChannelId}, Blog {BlogName}";
    }
}
