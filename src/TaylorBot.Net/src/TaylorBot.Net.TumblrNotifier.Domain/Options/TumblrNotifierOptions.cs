using System;

namespace TaylorBot.Net.TumblrNotifier.Domain.Options
{
    public class TumblrNotifierOptions
    {
        public TimeSpan TimeSpanBetweenRequests { get; set; }
        public string TumblrPostEmbedIconUrl { get; set; }
        public string TumblrPostEmbedColor { get; set; }
        public string TumblrLinkPostThumbnailUrl { get; set; }
        public string TumblrDefaultVideoThumbnailUrl { get; internal set; }
        public string TumblrTextPostThumbnailUrl { get; internal set; }
        public string TumblrQuotePostThumbnailUrl { get; internal set; }
        public string TumblrAudioPostThumbnailUrl { get; internal set; }
    }
}
