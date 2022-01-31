using System;

namespace TaylorBot.Net.YoutubeNotifier.Domain.Options
{
    public class YoutubeNotifierOptions
    {
        public TimeSpan TimeSpanBetweenRequests { get; set; }
        public string YoutubePostEmbedIconUrl { get; set; } = null!;
        public string YoutubePostEmbedColor { get; set; } = null!;
    }
}
