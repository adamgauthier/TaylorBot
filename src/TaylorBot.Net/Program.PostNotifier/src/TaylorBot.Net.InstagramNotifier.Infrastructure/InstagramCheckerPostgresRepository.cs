using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.InstagramNotifier.Domain;

namespace TaylorBot.Net.InstagramNotifier.Infrastructure
{
    public class InstagramCheckerPostgresRepository : IInstagramCheckerRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public InstagramCheckerPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        private class InstagramCheckerDto
        {
            public string guild_id { get; set; }
            public string channel_id { get; set; }
            public string instagram_username { get; set; }
            public string last_post_code { get; set; }
            public DateTime last_taken_at { get; set; }
        }

        public async ValueTask<IEnumerable<InstagramChecker>> GetInstagramCheckersAsync()
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var checkers = await connection.QueryAsync<InstagramCheckerDto>("SELECT * FROM checkers.instagram_checker;");
            return checkers.Select(checker => new InstagramChecker(
                guildId: new SnowflakeId(checker.guild_id),
                channelId: new SnowflakeId(checker.channel_id),
                instagramUsername: checker.instagram_username,
                lastPostCode: checker.last_post_code,
                lastPostTakenAt: checker.last_taken_at
            ));
        }

        public async ValueTask UpdateLastPostAsync(InstagramChecker instagramChecker, InstagramPost instagramPost)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE checkers.instagram_checker SET last_post_code = @LastPostCode, last_taken_at = @LastTakenAt
                WHERE instagram_username = @InstagramUsername AND guild_id = @GuildId AND channel_id = @ChannelId;",
                new
                {
                    InstagramUsername = instagramChecker.InstagramUsername,
                    GuildId = instagramChecker.GuildId.ToString(),
                    ChannelId = instagramChecker.ChannelId.ToString(),
                    LastPostCode = instagramPost.ShortCode,
                    LastTakenAt = instagramPost.TakenAt
                }
            );
        }
    }
}
