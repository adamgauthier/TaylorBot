using System;

namespace TaylorBot.Net.RedditNotifier.Domain.Options
{
    public class RedditNotifierOptions
    {
        public TimeSpan TimeSpanBetweenRequests { get; set; }
        public string RedditPostEmbedIconUrl { get; set; } = null!;
        public string RedditPostEmbedColor { get; set; } = null!;
        public string RedditPostEmbedSelfPostSpoilerDescription { get; set; } = null!;
        public string RedditPostEmbedLinkPostSpoilerThumbnailUrl { get; set; } = null!;
        public string RedditPostEmbedLinkPostNoThumbnailUrl { get; set; } = null!;
    }
}
