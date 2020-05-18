using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Reminder.Domain;
using TaylorBot.Net.Reminder.Infrastructure.Models;

namespace TaylorBot.Net.Reminder.Infrastructure
{
    public class ReminderRepository : IReminderRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public ReminderRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async Task<IEnumerable<Domain.Reminder>> GetExpiredRemindersAsync()
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var reminders = await connection.QueryAsync<ReminderDto>(
                "SELECT reminder_id, user_id, reminder_text, created_at FROM users.reminders WHERE CURRENT_TIMESTAMP > remind_at;"
            );

            return reminders.Select(r => new Domain.Reminder(r.reminder_id, new SnowflakeId(r.user_id), r.created_at, r.reminder_text));
        }

        public async Task RemoveReminderAsync(Domain.Reminder reminder)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"DELETE FROM users.reminders WHERE reminder_id = @ReminderId;",
                new
                {
                    ReminderId = reminder.ReminderId
                }
            );
        }
    }
}
