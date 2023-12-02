namespace TaylorBot.Net.InstagramNotifier.Domain.Options;

public class InstagramNotifierOptions
{
    public TimeSpan TimeSpanBetweenRequests { get; set; }
    public string InstagramPostEmbedIconUrl { get; set; } = null!;
    public string InstagramPostEmbedColor { get; set; } = null!;
}
