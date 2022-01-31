using System;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.YoutubeNotifier.Domain
{
    public class YoutubeChecker
    {
        public SnowflakeId GuildId { get; }
        public SnowflakeId ChannelId { get; }
        public string PlaylistId { get; }
        public string? LastVideoId { get; }
        public DateTimeOffset? LastPublishedAt { get; }

        public YoutubeChecker(SnowflakeId guildId, SnowflakeId channelId, string playlistId, string? lastVideoId, DateTimeOffset? lastPublishedAt)
        {
            GuildId = guildId;
            ChannelId = channelId;
            PlaylistId = playlistId;
            LastVideoId = lastVideoId;
            LastPublishedAt = lastPublishedAt;
        }

        public override string ToString()
        {
            return $"Youtube Checker for Guild {GuildId}, Channel {ChannelId}, Playlist {PlaylistId}";
        }
    }
}
