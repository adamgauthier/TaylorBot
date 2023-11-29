using Dapper;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Reminder.Domain;

namespace TaylorBot.Net.Reminder.Infrastructure
{
    public class ReminderRepository : IReminderRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public ReminderRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        private class ReminderDto
        {
            public Guid reminder_id { get; set; }
            public string user_id { get; set; } = null!;
            public string reminder_text { get; set; } = null!;
            public DateTime created_at { get; set; }
        }

        public async ValueTask<IReadOnlyCollection<Domain.Reminder>> GetExpiredRemindersAsync()
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            var reminders = await connection.QueryAsync<ReminderDto>(
                "SELECT reminder_id, user_id, reminder_text, created_at FROM users.reminders WHERE CURRENT_TIMESTAMP > remind_at;"
            );

            return reminders.Select(r => new Domain.Reminder(
                r.reminder_id,
                new SnowflakeId(r.user_id),
                r.created_at,
                r.reminder_text
            )).ToList();
        }

        public async ValueTask RemoveReminderAsync(Domain.Reminder reminder)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

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
