namespace TaylorBot.Net.EntityTracker.Domain.Options;

public record EntityTrackerOptions
{
    public TimeSpan TimeSpanBetweenGuildProcessedInReady { get; set; }
    public bool UseRedisCache { get; set; }
}
