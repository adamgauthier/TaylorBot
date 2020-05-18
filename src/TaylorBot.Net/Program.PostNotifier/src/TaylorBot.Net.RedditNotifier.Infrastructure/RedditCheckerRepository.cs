using Dapper;
using Reddit.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.RedditNotifier.Infrastructure.Models;

namespace TaylorBot.Net.RedditNotifier.Infrastructure
{
    public class RedditCheckerRepository : IRedditCheckerRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public RedditCheckerRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async Task<IEnumerable<RedditChecker>> GetRedditCheckersAsync()
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var checkers = await connection.QueryAsync<RedditCheckerDto>("SELECT * FROM checkers.reddit_checker;");

            return checkers.Select(checker => new RedditChecker(
                guildId: new SnowflakeId(checker.guild_id),
                channelId: new SnowflakeId(checker.channel_id),
                subredditName: checker.subreddit,
                lastPostId: checker.last_post_id,
                lastPostCreatedAt: checker.last_created
            ));
        }

        public async Task UpdateLastPostAsync(RedditChecker redditChecker, Post redditPost)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE checkers.reddit_checker SET last_post_id = @LastPostId, last_created = @LastCreated
                      WHERE subreddit = @SubredditName AND guild_id = @GuildId AND channel_id = @ChannelId;",
                new
                {
                    SubredditName = redditChecker.SubredditName,
                    GuildId = redditChecker.GuildId.ToString(),
                    ChannelId = redditChecker.ChannelId.ToString(),
                    LastPostId = redditPost.Id,
                    LastCreated = redditPost.Created
                }
            );
        }
    }
}
