namespace TaylorBot.Net.TumblrNotifier.Domain.Options
{
    public class TumblrNotifierOptions
    {
        public TimeSpan TimeSpanBetweenRequests { get; set; }
        public string TumblrPostEmbedIconUrl { get; set; } = null!;
        public string TumblrPostEmbedColor { get; set; } = null!;
        public string TumblrLinkPostThumbnailUrl { get; set; } = null!;
        public string TumblrDefaultVideoThumbnailUrl { get; set; } = null!;
        public string TumblrTextPostThumbnailUrl { get; set; } = null!;
        public string TumblrQuotePostThumbnailUrl { get; set; } = null!;
        public string TumblrAudioPostThumbnailUrl { get; set; } = null!;
    }
}
