using Dapper;
using Google.Apis.YouTube.v3.Data;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.YoutubeNotifier.Domain;

namespace TaylorBot.Net.YoutubeNotifier.Infrastructure;

public class YoutubeCheckerPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IYoutubeCheckerRepository
{
    private record YoutubeCheckerDto(string guild_id, string channel_id, string playlist_id, string? last_video_id, DateTime? last_published_at);

    public async ValueTask<IReadOnlyCollection<YoutubeChecker>> GetYoutubeCheckersAsync()
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

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
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE checkers.youtube_checker SET
                last_video_id = @LastVideoId,
                last_published_at = CASE
                    WHEN @LastPublishedAt IS NULL
                    THEN last_published_at
                    ELSE @LastPublishedAt
                END
            WHERE playlist_id = @PlaylistId AND guild_id = @GuildId AND channel_id = @ChannelId;
            """,
            new
            {
                PlaylistId = youtubeChecker.PlaylistId,
                GuildId = youtubeChecker.GuildId.ToString(),
                ChannelId = youtubeChecker.ChannelId.ToString(),
                LastVideoId = youtubePost.ResourceId.VideoId,
                LastPublishedAt = youtubePost.PublishedAtDateTimeOffset?.ToUniversalTime(),
            }
        );
    }
}
