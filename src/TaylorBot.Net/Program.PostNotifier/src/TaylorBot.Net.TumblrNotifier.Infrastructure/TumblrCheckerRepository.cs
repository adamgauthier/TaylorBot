using Dapper;
using DontPanic.TumblrSharp.Client;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.TumblrNotifier.Domain;
using TaylorBot.Net.TumblrNotifier.Infrastructure.Models;

namespace TaylorBot.Net.TumblrNotifier.Infrastructure
{
    public class TumblrCheckerRepository : PostgresRepository, ITumblrCheckerRepository
    {
        public TumblrCheckerRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task<IEnumerable<TumblrChecker>> GetTumblrCheckersAsync()
        {
            using (var connection = Connection)
            {
                connection.Open();
                var checkers = await connection.QueryAsync<TumblrCheckerDto>("SELECT * FROM checkers.tumblr_checker;");
                return checkers.Select(checker => new TumblrChecker(
                    guildId: new SnowflakeId(checker.guild_id),
                    channelId: new SnowflakeId(checker.channel_id),
                    blogName: checker.tumblr_user,
                    lastPostShortUrl: checker.last_link
                ));
            }
        }

        public async Task UpdateLastPostAsync(TumblrChecker tumblrChecker, BasePost tumblrPost)
        {
            using (var connection = Connection)
            {
                connection.Open();
                await connection.ExecuteAsync(
                    @"UPDATE checkers.tumblr_checker SET last_link = @LastLink
                      WHERE tumblr_user = @TumblrUser AND guild_id = @GuildId AND channel_id = @ChannelId;",
                    new
                    {
                        TumblrUser = tumblrChecker.BlogName,
                        GuildId = tumblrChecker.GuildId.ToString(),
                        ChannelId = tumblrChecker.ChannelId.ToString(),
                        LastLink = tumblrPost.ShortUrl
                    }
                );
            }
        }
    }
}
