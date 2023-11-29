namespace TaylorBot.Net.MinutesTracker.Domain.Options
{
    public class MinutesTrackerOptions
    {
        public uint MinutesToAdd { get; set; }
        public TimeSpan MinimumTimeSpanSinceLastSpoke { get; set; }
        public uint MinutesRequiredForReward { get; set; }
        public uint PointsReward { get; set; }
        public TimeSpan TimeSpanBetweenMinutesAdding { get; set; }
    }
}
