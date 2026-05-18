namespace TaylorBot.Net.MessagesTracker.Domain.Options;

public class MessagesTrackerOptions
{
    public TimeSpan TimeSpanBetweenPersistingTextChannelMessages { get; set; }
    public TimeSpan TimeSpanBetweenPersistingMemberMessagesAndWords { get; set; }
    public TimeSpan TimeSpanBetweenPersistingLastSpoke { get; set; }
}
