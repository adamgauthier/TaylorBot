using System;

namespace TaylorBot.Net.InstagramNotifier.Domain.Options
{
    public class InstagramNotifierOptions
    {
        public TimeSpan TimeSpanBetweenRequests { get; set; }
        public string InstagramPostEmbedIconUrl { get; set; }
        public string InstagramPostEmbedColor { get; set; }
    }
}
