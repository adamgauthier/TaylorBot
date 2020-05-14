using System;

namespace TaylorBot.Net.Reminder.Infrastructure.Models
{
    public class ReminderDto
    {
        public Guid reminder_id { get; set; }
        public string user_id { get; set; }
        public string reminder_text { get; set; }
        public DateTime created_at { get; set; }
    }
}
