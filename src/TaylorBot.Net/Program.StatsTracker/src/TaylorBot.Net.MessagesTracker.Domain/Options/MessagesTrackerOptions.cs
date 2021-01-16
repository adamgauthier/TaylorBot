using System;

namespace TaylorBot.Net.MinutesTracker.Domain.Options
{
    public class MessagesTrackerOptions
    {
        public TimeSpan TimeSpanBetweenPersistingTextChannelMessages { get; set; }
    }
}
