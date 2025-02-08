using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Domain;

public record Reminder(Guid Id, DateTimeOffset RemindAt, string Text);

public interface IReminderRepository
{
    ValueTask<long> GetReminderCountAsync(DiscordUser user);
    ValueTask<IList<Reminder>> GetRemindersAsync(DiscordUser user);
    ValueTask AddReminderAsync(DiscordUser user, DateTimeOffset remindAt, string text);
    ValueTask ClearReminderAsync(Guid reminderId);
    ValueTask ClearAllRemindersAsync(DiscordUser user);
}
