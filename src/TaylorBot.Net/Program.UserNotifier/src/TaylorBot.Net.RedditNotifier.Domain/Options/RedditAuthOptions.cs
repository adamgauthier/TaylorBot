namespace TaylorBot.Net.RedditNotifier.Domain.Options
{
    public class RedditAuthOptions
    {
        public string AppId { get; set; } = null!;
        public string AppSecret { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
