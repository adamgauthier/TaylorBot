using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.DailyPayout.Domain
{
    public class MessagePriority
    {
        public DateTimeOffset From { get; }
        public DateTimeOffset To { get; }

        public MessagePriority(DateTimeOffset from, DateTimeOffset to)
        {
            From = from;
            To = to;
        }
    }

    public class MessageOfTheDay
    {
        public string Message { get; }
        public MessagePriority? MessagePriority { get; }

        public MessageOfTheDay(string message, MessagePriority? messagePriority)
        {
            Message = message;
            MessagePriority = messagePriority;
        }
    }

    public interface IMessageOfTheDayRepository
    {
        ValueTask<IReadOnlyList<MessageOfTheDay>> GetAllMessagesAsync();
    }
}
