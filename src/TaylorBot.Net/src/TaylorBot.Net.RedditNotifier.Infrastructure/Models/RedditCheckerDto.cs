using System;

namespace TaylorBot.Net.RedditNotifier.Infrastructure.Models
{
    public class RedditCheckerDto
    {
        public string guild_id { get; set; }
        public string channel_id { get; set; }
        public string subreddit { get; set; }
        public string last_post_id { get; set; }
        public DateTime last_created { get; set; }
    }
}
