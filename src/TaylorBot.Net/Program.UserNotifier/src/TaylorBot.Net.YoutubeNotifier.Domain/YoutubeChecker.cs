using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.YoutubeNotifier.Domain;

public class YoutubeChecker(SnowflakeId guildId, SnowflakeId channelId, string playlistId, string? lastVideoId, DateTimeOffset? lastPublishedAt)
{
    public SnowflakeId GuildId { get; } = guildId;
    public SnowflakeId ChannelId { get; } = channelId;
    public string PlaylistId { get; } = playlistId;
    public string? LastVideoId { get; } = lastVideoId;
    public DateTimeOffset? LastPublishedAt { get; } = lastPublishedAt;

    public override string ToString()
    {
        return $"Youtube Checker for Guild {GuildId}, Channel {ChannelId}, Playlist {PlaylistId}";
    }
}
