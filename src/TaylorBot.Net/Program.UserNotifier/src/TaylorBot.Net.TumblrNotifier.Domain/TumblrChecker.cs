using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.TumblrNotifier.Domain
{
    public class TumblrChecker
    {
        public SnowflakeId GuildId { get; }
        public SnowflakeId ChannelId { get; }
        public string BlogName { get; }
        public string? LastPostShortUrl { get; }

        public TumblrChecker(SnowflakeId guildId, SnowflakeId channelId, string blogName, string? lastPostShortUrl)
        {
            GuildId = guildId;
            ChannelId = channelId;
            BlogName = blogName;
            LastPostShortUrl = lastPostShortUrl;
        }

        public override string ToString()
        {
            return $"Tumblr Checker for Guild {GuildId}, Channel {ChannelId}, Blog {BlogName}";
        }
    }
}
