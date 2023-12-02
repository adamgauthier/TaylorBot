using Discord;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Domain;

public record Reminder(Guid Id, DateTimeOffset RemindAt, string Text);

public interface IReminderRepository
{
    ValueTask<long> GetReminderCountAsync(IUser user);
    ValueTask<IList<Reminder>> GetRemindersAsync(IUser user);
    ValueTask AddReminderAsync(IUser user, DateTimeOffset remindAt, string text);
    ValueTask ClearReminderAsync(Reminder reminder);
    ValueTask ClearAllRemindersAsync(IUser user);
}
