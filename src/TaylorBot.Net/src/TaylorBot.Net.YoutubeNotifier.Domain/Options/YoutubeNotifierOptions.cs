using System;

namespace TaylorBot.Net.YoutubeNotifier.Domain.Options
{
    public class YoutubeNotifierOptions
    {
        public TimeSpan TimeSpanBetweenRequests { get; set; }
        public string YoutubePostEmbedIconUrl { get; set; }
        public string YoutubePostEmbedColor { get; set; }
    }
}
