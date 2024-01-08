using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.RedditNotifier.Domain;

public class RedditChecker(SnowflakeId guildId, SnowflakeId channelId, string subredditName, string? lastPostId, DateTime? lastPostCreatedAt)
{
    public SnowflakeId GuildId { get; } = guildId;
    public SnowflakeId ChannelId { get; } = channelId;
    public string SubredditName { get; } = subredditName;
    public string? LastPostId { get; } = lastPostId;
    public DateTime? LastPostCreatedAt { get; } = lastPostCreatedAt;

    public override string ToString()
    {
        return $"Reddit Checker for Guild {GuildId}, Channel {ChannelId}, Subreddit {SubredditName}";
    }
}
