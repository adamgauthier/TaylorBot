namespace TaylorBot.Net.TumblrNotifier.Domain.Options
{
    public class TumblrAuthOptions
    {
        public string ConsumerKey { get; set; } = null!;
        public string ConsumerSecret { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string TokenSecret { get; set; } = null!;
    }
}
