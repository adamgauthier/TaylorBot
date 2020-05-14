using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.Reminder.Domain
{
    public interface IReminderRepository
    {
        Task<IEnumerable<Reminder>> GetExpiredRemindersAsync();
        Task RemoveReminderAsync(Reminder reminder);
    }
}
