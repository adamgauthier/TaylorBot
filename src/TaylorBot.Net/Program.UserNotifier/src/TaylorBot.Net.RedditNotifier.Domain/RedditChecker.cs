using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.RedditNotifier.Domain;

public class RedditChecker
{
    public SnowflakeId GuildId { get; }
    public SnowflakeId ChannelId { get; }
    public string SubredditName { get; }
    public string? LastPostId { get; }
    public DateTime? LastPostCreatedAt { get; }

    public RedditChecker(SnowflakeId guildId, SnowflakeId channelId, string subredditName, string? lastPostId, DateTime? lastPostCreatedAt)
    {
        GuildId = guildId;
        ChannelId = channelId;
        SubredditName = subredditName;
        LastPostId = lastPostId;
        LastPostCreatedAt = lastPostCreatedAt;
    }

    public override string ToString()
    {
        return $"Reddit Checker for Guild {GuildId}, Channel {ChannelId}, Subreddit {SubredditName}";
    }
}
