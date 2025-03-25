using Dapper;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Reminder.Domain;

namespace TaylorBot.Net.Reminder.Infrastructure;

public class ReminderRepository(PostgresConnectionFactory postgresConnectionFactory) : IReminderRepository
{
    private sealed record ReminderDto(Guid reminder_id, string user_id, string reminder_text, DateTime created_at);

    public async ValueTask<IReadOnlyCollection<Domain.Reminder>> GetDueRemindersAsync()
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var reminders = await connection.QueryAsync<ReminderDto>(
            """
            SELECT reminder_id, user_id, reminder_text, created_at
            FROM users.reminders
            WHERE CURRENT_TIMESTAMP > remind_at;
            """
        );

        return [.. reminders.Select(r => new Domain.Reminder(
            r.reminder_id,
            new SnowflakeId(r.user_id),
            r.created_at,
            r.reminder_text
        ))];
    }

    public async ValueTask RemoveReminderAsync(Domain.Reminder reminder)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"DELETE FROM users.reminders WHERE reminder_id = @ReminderId;",
            new
            {
                ReminderId = reminder.ReminderId,
            }
        );
    }
}
