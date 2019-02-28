using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using Reddit.Controllers;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.RedditNotifier.Infrastructure.Models;

namespace TaylorBot.Net.RedditNotifier.Infrastructure
{
    public class RedditCheckerRepository : IRedditCheckerRepository
    {
        private IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor;

        public RedditCheckerRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor)
        {
            this.optionsMonitor = optionsMonitor;
        }

        private IDbConnection Connection
        {
            get
            {
                var options = optionsMonitor.CurrentValue;
                return new NpgsqlConnection(
                    $"Server={options.Host};Port={options.Port};Username={options.Username};Password={options.Password};Database={options.Database}"
                );
            }
        }

        public async Task<IEnumerable<RedditChecker>> GetRedditCheckersAsync()
        {
            using (var connection = Connection)
            {
                connection.Open();
                var checkers = await connection.QueryAsync<RedditCheckerDto>("SELECT * FROM checkers.reddit_checker;");
                return checkers.Select(checker => new RedditChecker(
                    guildId: new SnowflakeId(checker.guild_id),
                    channelId: new SnowflakeId(checker.channel_id),
                    subredditName: checker.subreddit,
                    lastPostId: checker.last_post_id,
                    lastPostCreatedAt: checker.last_created
                ));
            }
        }

        public async Task UpdateLastPostAsync(RedditChecker redditChecker, Post redditPost)
        {
            using (var connection = Connection)
            {
                connection.Open();
                await connection.ExecuteAsync(
                    @"UPDATE checkers.reddit_checker SET last_post_id = @LastPostId, last_created = @LastCreated
                      WHERE subreddit = @SubredditName AND guild_id = @GuildId AND channel_id = @ChannelId;",
                    new
                    {
                        SubredditName = redditPost.Subreddit,
                        GuildId = redditChecker.GuildId.ToString(),
                        ChannelId = redditChecker.ChannelId.ToString(),
                        LastPostId = redditPost.Id,
                        LastCreated = redditPost.Created
                    }
                );
            }
        }
    }
}
