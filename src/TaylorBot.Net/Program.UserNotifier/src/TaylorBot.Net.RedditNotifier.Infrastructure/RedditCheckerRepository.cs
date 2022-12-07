using Dapper;
using Reddit.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.RedditNotifier.Domain;

namespace TaylorBot.Net.RedditNotifier.Infrastructure
{
    public class RedditCheckerRepository : IRedditCheckerRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public RedditCheckerRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

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
            await using var connection = _postgresConnectionFactory.CreateConnection();

            var checkers = await connection.QueryAsync<RedditCheckerDto>(
                "SELECT guild_id, channel_id, subreddit, last_post_id, last_created FROM checkers.reddit_checker;"
            );

            return checkers.Select(checker => new RedditChecker(
                guildId: new SnowflakeId(checker.guild_id),
                channelId: new SnowflakeId(checker.channel_id),
                subredditName: checker.subreddit,
                lastPostId: checker.last_post_id,
                lastPostCreatedAt: checker.last_created
            )).ToList();
        }

        public async ValueTask UpdateLastPostAsync(RedditChecker redditChecker, Post redditPost)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE checkers.reddit_checker SET last_post_id = @LastPostId, last_created = @LastCreated
                WHERE subreddit = @SubredditName AND guild_id = @GuildId AND channel_id = @ChannelId;",
                new
                {
                    SubredditName = redditChecker.SubredditName,
                    GuildId = redditChecker.GuildId.ToString(),
                    ChannelId = redditChecker.ChannelId.ToString(),
                    LastPostId = redditPost.Id,
                    LastCreated = redditPost.Created.ToUniversalTime(),
                }
            );
        }
    }
}
