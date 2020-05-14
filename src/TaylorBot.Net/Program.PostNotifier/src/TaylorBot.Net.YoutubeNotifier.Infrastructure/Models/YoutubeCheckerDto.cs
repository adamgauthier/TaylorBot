using System;

namespace TaylorBot.Net.YoutubeNotifier.Infrastructure.Models
{
    public class YoutubeCheckerDto
    {
        public string guild_id { get; set; }
        public string channel_id { get; set; }
        public string playlist_id { get; set; }
        public string last_video_id { get; set; }
        public DateTimeOffset? last_published_at { get; set; }
    }
}
