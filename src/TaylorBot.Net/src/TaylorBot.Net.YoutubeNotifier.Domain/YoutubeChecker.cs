using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.YoutubeNotifier.Domain
{
    public class YoutubeChecker
    {
        public SnowflakeId GuildId { get; }
        public SnowflakeId ChannelId { get; }
        public string PlaylistId { get; }
        public string LastVideoId { get; }

        public YoutubeChecker(SnowflakeId guildId, SnowflakeId channelId, string playlistId, string lastVideoId)
        {
            GuildId = guildId;
            ChannelId = channelId;
            PlaylistId = playlistId;
            LastVideoId = lastVideoId;
        }

        public override string ToString()
        {
            return $"Youtube Checker for Guild {GuildId}, Channel {ChannelId}, Playlist {PlaylistId}";
        }
    }
}
