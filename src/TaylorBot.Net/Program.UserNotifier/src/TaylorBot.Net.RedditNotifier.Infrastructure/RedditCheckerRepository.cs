using Dapper;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.RedditNotifier.Domain;

namespace TaylorBot.Net.RedditNotifier.Infrastructure;

public class RedditCheckerRepository(PostgresConnectionFactory postgresConnectionFactory) : IRedditCheckerRepository
{
    private class RedditCheckerDto
    {
        public string guild_id { get; set; } = null!;
        public string channel_id { get; set; } = null!;
        public string subreddit { get; set; } = null!;
        public string? last_post_id { get; set; }
        public DateTime? last_created { get; set; }
    }

    public async ValueTask<IReadOnlyCollection<RedditChecker>> GetRedditCheckersAsync()
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var checkers = await connection.QueryAsync<RedditCheckerDto>(
            "SELECT guild_id, channel_id, subreddit, last_post_id, last_created FROM checkers.reddit_checker;"
        );

        return checkers.Select(checker => new RedditChecker(
            GuildId: checker.guild_id,
            ChannelId: checker.channel_id,
            SubredditName: checker.subreddit,
            LastPostId: checker.last_post_id,
            LastPostCreatedAt: checker.last_created
        )).ToList();
    }

    public async ValueTask UpdateLastPostAsync(RedditChecker redditChecker, RedditPost redditPost)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE checkers.reddit_checker SET last_post_id = @LastPostId, last_created = @LastCreated
            WHERE subreddit = @SubredditName AND guild_id = @GuildId AND channel_id = @ChannelId;
            """,
            new
            {
                SubredditName = redditChecker.SubredditName,
                GuildId = $"{redditChecker.GuildId}",
                ChannelId = $"{redditChecker.ChannelId}",
                LastPostId = redditPost.id,
                LastCreated = redditPost.CreatedAt,
            }
        );
    }
}
