using System;

namespace TaylorBot.Net.RedditNotifier.Domain.Options
{
    public class RedditNotifierOptions
    {
        public TimeSpan TimeSpanBetweenRequests { get; set; }
        public string RedditPostEmbedIconUrl { get; set; }
        public string RedditPostEmbedColor { get; set; }
        public string RedditPostEmbedSelfPostSpoilerDescription { get; set; }
        public string RedditPostEmbedLinkPostSpoilerThumbnailUrl { get; set; }
        public string RedditPostEmbedLinkPostNoThumbnailUrl { get; set; }
    }
}
