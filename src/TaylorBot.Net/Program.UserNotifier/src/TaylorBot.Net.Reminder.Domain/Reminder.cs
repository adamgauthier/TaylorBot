using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Reminder.Domain;

public class Reminder(Guid reminderId, SnowflakeId userId, DateTimeOffset createdAt, string reminderText)
{
    public Guid ReminderId { get; } = reminderId;
    public SnowflakeId UserId { get; } = userId;
    public DateTimeOffset CreatedAt { get; } = createdAt;
    public string ReminderText { get; } = reminderText;

    public override string ToString()
    {
        return $"Reminder {ReminderId} for User ID {UserId}, Created At {CreatedAt}, Text '{ReminderText.EscapeNewLines()}'";
    }
}
