using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.RedditNotifier.Domain;

public record RedditChecker(SnowflakeId GuildId, SnowflakeId ChannelId, string SubredditName, string? LastPostId, DateTime? LastPostCreatedAt)
{
    public override string ToString()
    {
        return $"Reddit Checker for Guild {GuildId}, Channel {ChannelId}, Subreddit {SubredditName}";
    }
}
