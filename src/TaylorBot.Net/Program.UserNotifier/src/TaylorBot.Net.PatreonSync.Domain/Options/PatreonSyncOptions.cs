using System;

namespace TaylorBot.Net.PatreonSync.Domain.Options
{
    public class PatreonSyncOptions
    {
        public bool Enabled { get; set; }
        public string ApiKey { get; set; } = null!;
        public uint CampaignId { get; set; }
        public TimeSpan TimeSpanBetweenSyncs { get; set; }
        public TimeSpan TimeSpanBetweenMessages { get; set; }
    }
}
