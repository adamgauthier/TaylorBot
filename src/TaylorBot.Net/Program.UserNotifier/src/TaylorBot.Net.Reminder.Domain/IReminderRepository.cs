namespace TaylorBot.Net.Reminder.Domain;

public interface IReminderRepository
{
    ValueTask<IReadOnlyCollection<Reminder>> GetExpiredRemindersAsync();
    ValueTask RemoveReminderAsync(Reminder reminder);
}
