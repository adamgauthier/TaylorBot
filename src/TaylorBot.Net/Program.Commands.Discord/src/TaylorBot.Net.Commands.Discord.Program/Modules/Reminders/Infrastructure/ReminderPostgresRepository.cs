using Dapper;
using TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Infrastructure;

public class ReminderPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IReminderRepository
{
    public async ValueTask<long> GetReminderCountAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<int>(
            """
            SELECT COUNT(*) FROM users.reminders
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }

    private record ReminderDto(Guid reminder_id, DateTime remind_at, string reminder_text);

    public async ValueTask<IList<Reminder>> GetRemindersAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var reminders = await connection.QueryAsync<ReminderDto>(
            """
            SELECT reminder_id, remind_at, reminder_text
            FROM users.reminders
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );

        return reminders.Select(r => new Reminder(r.reminder_id, r.remind_at, r.reminder_text)).ToList();
    }

    public async ValueTask AddReminderAsync(DiscordUser user, DateTimeOffset remindAt, string text)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO users.reminders (user_id, remind_at, reminder_text)
            VALUES (@UserId, @RemindAt, @ReminderText);
            """,
            new
            {
                UserId = $"{user.Id}",
                RemindAt = remindAt,
                ReminderText = text,
            }
        );
    }

    public async ValueTask ClearReminderAsync(Guid reminderId)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"DELETE FROM users.reminders WHERE reminder_id = @ReminderId;",
            new
            {
                ReminderId = reminderId,
            }
        );
    }

    public async ValueTask ClearAllRemindersAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"DELETE FROM users.reminders WHERE user_id = @UserId;",
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }
}
