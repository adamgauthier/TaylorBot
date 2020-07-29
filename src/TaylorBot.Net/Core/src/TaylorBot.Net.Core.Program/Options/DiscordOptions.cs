namespace TaylorBot.Net.Core.Program.Options
{
    public class DiscordOptions
    {
        public string Token { get; set; } = null!;
        public uint ShardCount { get; set; }
        public uint? MessageCacheSize { get; set; }
        public bool? ExclusiveBulkDelete { get; set; }
    }
}
