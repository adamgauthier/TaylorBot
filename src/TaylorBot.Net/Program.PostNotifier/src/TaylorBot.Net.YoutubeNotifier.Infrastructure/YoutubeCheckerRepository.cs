using Dapper;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.YoutubeNotifier.Domain;

namespace TaylorBot.Net.YoutubeNotifier.Infrastructure
{
    public class YoutubeCheckerRepository : IYoutubeCheckerRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public YoutubeCheckerRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        private class YoutubeCheckerDto
        {
            public string guild_id { get; set; }
            public string channel_id { get; set; }
            public string playlist_id { get; set; }
            public string last_video_id { get; set; }
            public DateTimeOffset? last_published_at { get; set; }
        }

        public async ValueTask<IReadOnlyCollection<YoutubeChecker>> GetYoutubeCheckersAsync()
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var checkers = await connection.QueryAsync<YoutubeCheckerDto>(
                "SELECT guild_id, channel_id, playlist_id, last_video_id, last_published_at FROM checkers.youtube_checker;"
            );

            return checkers.Select(checker => new YoutubeChecker(
                guildId: new SnowflakeId(checker.guild_id),
                channelId: new SnowflakeId(checker.channel_id),
                playlistId: checker.playlist_id,
                lastVideoId: checker.last_video_id,
                lastPublishedAt: checker.last_published_at
            )).ToList();
        }

        public async ValueTask UpdateLastPostAsync(YoutubeChecker youtubeChecker, PlaylistItemSnippet youtubePost)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE checkers.youtube_checker SET
                    last_video_id = @LastVideoId,
                    last_published_at = @LastPublishedAt
                WHERE playlist_id = @PlaylistId AND guild_id = @GuildId AND channel_id = @ChannelId;",
                new
                {
                    PlaylistId = youtubeChecker.PlaylistId,
                    GuildId = youtubeChecker.GuildId.ToString(),
                    ChannelId = youtubeChecker.ChannelId.ToString(),
                    LastVideoId = youtubePost.ResourceId.VideoId,
                    LastPublishedAt = youtubePost.PublishedAt.Value
                }
            );
        }
    }
}
