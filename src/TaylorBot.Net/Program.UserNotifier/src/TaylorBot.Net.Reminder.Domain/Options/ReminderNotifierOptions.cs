namespace TaylorBot.Net.Reminder.Domain.Options
{
    public class ReminderNotifierOptions
    {
        public TimeSpan TimeSpanBetweenReminderChecks { get; set; }
        public TimeSpan TimeSpanBetweenMessages { get; set; }
    }
}
