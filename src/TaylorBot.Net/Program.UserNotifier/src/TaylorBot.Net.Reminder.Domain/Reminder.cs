using System;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Reminder.Domain
{
    public class Reminder
    {
        public Guid ReminderId { get; }
        public SnowflakeId UserId { get; }
        public DateTimeOffset CreatedAt { get; }
        public string ReminderText { get; }

        public Reminder(Guid reminderId, SnowflakeId userId, DateTimeOffset createdAt, string reminderText)
        {
            ReminderId = reminderId;
            UserId = userId;
            CreatedAt = createdAt;
            ReminderText = reminderText;
        }

        public override string ToString()
        {
            return $"Reminder {ReminderId} for User ID {UserId}, Created At {CreatedAt}, Text '{ReminderText.EscapeNewLines()}'";
        }
    }
}
