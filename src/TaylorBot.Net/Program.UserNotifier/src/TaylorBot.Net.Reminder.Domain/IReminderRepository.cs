namespace TaylorBot.Net.Reminder.Domain;

public interface IReminderRepository
{
    ValueTask<IReadOnlyCollection<Reminder>> GetDueRemindersAsync();
    ValueTask RemoveReminderAsync(Reminder reminder);
}
