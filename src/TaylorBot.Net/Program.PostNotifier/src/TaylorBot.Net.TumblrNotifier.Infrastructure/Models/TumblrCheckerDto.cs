namespace TaylorBot.Net.TumblrNotifier.Infrastructure.Models
{
    public class TumblrCheckerDto
    {
        public string guild_id { get; set; }
        public string channel_id { get; set; }
        public string tumblr_user { get; set; }
        public string last_link { get; set; }
    }
}
